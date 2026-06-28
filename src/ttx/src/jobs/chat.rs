pub mod twitch;

use std::collections::HashSet;
use std::sync::Arc;

use async_trait::async_trait;
use chrono::Duration;
use tokio::sync::mpsc;
use tokio_util::sync::CancellationToken;

use crate::creators::Creator;
use crate::creators::events::UpdateCreatorValueEvent;
use crate::data::Db;
use crate::error::Result;
use crate::events::{EventDispatcher, EventReceiver};
use crate::factories::ChatMonitorFactory;

#[derive(Debug, PartialEq, Eq)]
pub struct Message {
    pub slug: String,
    pub content: String,
}

#[async_trait]
pub trait ChatMonitorAdapter: Send + Sync {
    async fn start(
        &self,
        channels: Vec<String>,
        sink: mpsc::Sender<Message>,
        cancel: CancellationToken,
    ) -> Result<()>;
    async fn add(&self, channel: String) -> Result<bool>;
    async fn remove(&self, channel: String) -> Result<bool>;
}

#[async_trait]
pub trait MessageAnalyzer: Send + Sync {
    async fn analyze(&self, content: &str) -> Result<f64>;
}

#[derive(Debug, PartialEq, serde::Serialize, serde::Deserialize)]
pub struct CreatorStats {
    pub positive: f64,
    pub negative: f64,
    pub message_count: i64,
    pub creator_slug: String,
}

impl CreatorStats {
    pub fn new(creator_slug: String) -> Self {
        Self {
            positive: 0.0,
            negative: 0.0,
            message_count: 0,
            creator_slug,
        }
    }
}

#[async_trait]
pub trait StatsProcessor: Send + Sync {
    async fn process(&self, creator: &Creator, stats: Option<&CreatorStats>) -> Result<f64>;
}

#[async_trait]
pub trait CreatorStatsRepository: Send + Sync {
    async fn get_by_creator(&self, slug: &str) -> Result<CreatorStats>;
    async fn set_by_creator(&self, slug: &str, stats: CreatorStats) -> Result<()>;
    async fn get_all(&self, clear: bool) -> Result<Vec<CreatorStats>>;
    async fn clear(&self, slugs: &[String]) -> Result<()>;
}

type Monitors = Vec<Arc<dyn ChatMonitorAdapter>>;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct ChatShard {
    pub index: usize,
    pub size: usize,
}

impl ChatShard {
    pub const DEFAULT_SIZE: usize = 50;

    pub fn from_env() -> Option<Self> {
        let index = std::env::var("CHAT_SHARD_INDEX")
            .ok()?
            .trim()
            .parse()
            .ok()?;
        let size = std::env::var("CHAT_SHARD_SIZE")
            .ok()
            .and_then(|v| v.trim().parse().ok())
            .filter(|&n| n > 0)
            .unwrap_or(Self::DEFAULT_SIZE);
        Some(Self { index, size })
    }

    pub fn select(&self, ordered_slugs: &[String]) -> HashSet<String> {
        if self.size == 0 {
            return HashSet::new();
        }
        let start = self.index.saturating_mul(self.size);
        ordered_slugs
            .iter()
            .skip(start)
            .take(self.size)
            .cloned()
            .collect()
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct CreatorValuesJobOptions {
    pub delay: Duration,
    pub shard: Option<ChatShard>,
}

pub struct CreatorValueMonitorJob {
    options: CreatorValuesJobOptions,
    db: Db,
    chat_factory: Arc<ChatMonitorFactory>,
    analyzer: Arc<dyn MessageAnalyzer>,
    stats_repository: Arc<dyn CreatorStatsRepository>,
    stats_processor: Arc<dyn StatsProcessor>,
    dispatcher: Arc<dyn EventDispatcher>,
    receiver: Arc<dyn EventReceiver>,
}

impl CreatorValueMonitorJob {
    #[allow(clippy::too_many_arguments)]
    pub fn new(
        options: CreatorValuesJobOptions,
        db: Db,
        chat_factory: Arc<ChatMonitorFactory>,
        analyzer: Arc<dyn MessageAnalyzer>,
        stats_repository: Arc<dyn CreatorStatsRepository>,
        stats_processor: Arc<dyn StatsProcessor>,
        dispatcher: Arc<dyn EventDispatcher>,
        receiver: Arc<dyn EventReceiver>,
    ) -> Self {
        Self {
            options,
            db,
            chat_factory,
            analyzer,
            stats_repository,
            stats_processor,
            dispatcher,
            receiver,
        }
    }

    pub async fn run(&self, cancel: CancellationToken) -> Result<()> {
        let monitors: Monitors = self.chat_factory.create_all();
        if monitors.is_empty() {
            tracing::warn!("No chat monitors configured.");
            return Ok(());
        }

        let shard_channels = match self.options.shard {
            Some(shard) => {
                let mut creators = self.db.all_creators().await?;
                creators.sort_by_key(|c| c.id());
                let slugs: Vec<String> = creators
                    .iter()
                    .map(|c| c.user.slug.to_lowercase())
                    .collect();
                let selected = shard.select(&slugs);
                tracing::info!(
                    index = shard.index,
                    size = shard.size,
                    channels = selected.len(),
                    "Chat sharding enabled; monitoring only this shard's creators"
                );
                Some(selected)
            }
            None => None,
        };

        let (tx, rx) = mpsc::channel::<Message>(1024);
        for monitor in &monitors {
            monitor
                .start(Vec::new(), tx.clone(), cancel.clone())
                .await?;
        }
        drop(tx);

        let mut process = tokio::spawn(Self::process_messages(
            rx,
            self.db.clone(),
            self.analyzer.clone(),
            self.stats_repository.clone(),
            monitors.clone(),
        ));

        let mut watch = tokio::spawn(Self::watch_stream_status(
            self.receiver.clone(),
            self.db.clone(),
            monitors.clone(),
            shard_channels.clone(),
        ));

        let delay = self
            .options
            .delay
            .to_std()
            .unwrap_or_else(|_| std::time::Duration::from_secs(60));
        let mut digest = tokio::spawn(Self::digest_loop(
            delay,
            self.db.clone(),
            self.stats_repository.clone(),
            self.stats_processor.clone(),
            self.dispatcher.clone(),
            shard_channels,
            cancel.clone(),
        ));

        tokio::select! {
            _ = cancel.cancelled() => {}
            _ = &mut process => {}
            _ = &mut watch => {}
            _ = &mut digest => {}
        }

        process.abort();
        watch.abort();
        digest.abort();

        Ok(())
    }

    async fn process_messages(
        mut rx: mpsc::Receiver<Message>,
        db: Db,
        analyzer: Arc<dyn MessageAnalyzer>,
        stats_repository: Arc<dyn CreatorStatsRepository>,
        monitors: Monitors,
    ) {
        while let Some(message) = rx.recv().await {
            if let Err(err) =
                Self::parse_message(&db, &analyzer, &stats_repository, &monitors, message).await
            {
                tracing::error!(?err, "Error parsing message");
            }
        }
    }

    async fn parse_message(
        db: &Db,
        analyzer: &Arc<dyn MessageAnalyzer>,
        stats_repository: &Arc<dyn CreatorStatsRepository>,
        monitors: &Monitors,
        message: Message,
    ) -> Result<()> {
        let creator = db.creator_by_slug(&message.slug).await?;

        let Some(creator) = creator else {
            tracing::warn!("Channel {} not found in database. Removing.", message.slug);
            for monitor in monitors {
                monitor.remove(message.slug.clone()).await?;
            }
            return Ok(());
        };

        if !creator.stream_status.is_live {
            return Ok(());
        }

        let value = analyzer.analyze(&message.content).await?;
        let mut stats = stats_repository.get_by_creator(&message.slug).await?;

        stats.message_count += 1;
        if value > 0.0 {
            stats.positive += 1.0;
        } else if value < 0.0 {
            stats.negative += 1.0;
        }

        tracing::debug!(
            "Channel {} ({:.2}): {}",
            message.slug,
            value,
            message.content
        );
        stats_repository
            .set_by_creator(&message.slug, stats)
            .await?;
        Ok(())
    }

    async fn watch_stream_status(
        receiver: Arc<dyn EventReceiver>,
        db: Db,
        monitors: Monitors,
        shard: Option<HashSet<String>>,
    ) {
        let mut rx = receiver.subscribe("UpdateStreamStatusEvent").await;
        while let Some(value) = rx.recv().await {
            let Some(creator_id) = value.get("creator_id").and_then(|v| v.as_u64()) else {
                continue;
            };

            let creator = match db.creators_by_ids(&[creator_id]).await {
                Ok(mut creators) => creators.pop(),
                Err(err) => {
                    tracing::error!(?err, "Failed to load creator for stream update");
                    continue;
                }
            };
            let Some(creator) = creator else { continue };

            let Some(monitor) = monitors.first() else {
                continue;
            };
            let slug = creator.user.slug.to_lowercase();
            if let Some(shard) = &shard
                && !shard.contains(&slug)
            {
                continue;
            }
            let result = if creator.stream_status.is_live {
                monitor.add(slug).await
            } else {
                monitor.remove(slug).await
            };
            if let Err(err) = result {
                tracing::error!(?err, "Failed to update chat monitor channels");
            }
        }
    }

    async fn digest_loop(
        delay: std::time::Duration,
        db: Db,
        stats_repository: Arc<dyn CreatorStatsRepository>,
        stats_processor: Arc<dyn StatsProcessor>,
        dispatcher: Arc<dyn EventDispatcher>,
        shard: Option<HashSet<String>>,
        cancel: CancellationToken,
    ) {
        let mut timer = tokio::time::interval(delay);
        timer.tick().await;
        loop {
            tokio::select! {
                _ = cancel.cancelled() => {
                    tracing::info!("Digest loop shutting down.");
                    break;
                }
                _ = timer.tick() => {
                    if let Err(err) = Self::digest_all(
                        &db,
                        &stats_repository,
                        &stats_processor,
                        &dispatcher,
                        shard.as_ref(),
                    ).await {
                        tracing::error!(?err, "An error occurred during the periodic digest.");
                    }
                }
            }
        }
    }

    async fn digest_all(
        db: &Db,
        stats_repository: &Arc<dyn CreatorStatsRepository>,
        stats_processor: &Arc<dyn StatsProcessor>,
        dispatcher: &Arc<dyn EventDispatcher>,
        shard: Option<&HashSet<String>>,
    ) -> Result<()> {
        let mut creators = db.live_creators().await?;
        let all_stats: Vec<CreatorStats> = stats_repository.get_all(shard.is_none()).await?;
        let portfolio = db.portfolio();

        if let Some(shard) = shard {
            creators.retain(|c| shard.contains(&c.user.slug.to_lowercase()));
        }

        for creator in &mut creators {
            let stats = all_stats
                .iter()
                .find(|s| s.creator_slug == creator.user.slug);
            let net_change = stats_processor.process(creator, stats).await?;
            if net_change == 0.0 {
                continue;
            }

            let vote = creator.apply_net_change(net_change);
            db.update_creator_value(creator).await?;
            portfolio.store_vote(&vote).await?;
            dispatcher
                .dispatch(&UpdateCreatorValueEvent::create(&vote))
                .await?;

            tracing::debug!(
                "{} {} {}",
                creator.user.slug,
                if net_change > 0.0 { "gained" } else { "lost" },
                net_change
            );
        }

        if let Some(shard) = shard {
            let to_clear: Vec<String> = all_stats
                .iter()
                .map(|s| s.creator_slug.clone())
                .filter(|slug| shard.contains(&slug.to_lowercase()))
                .collect();
            stats_repository.clear(&to_clear).await?;
        }

        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::ChatShard;

    fn slugs(n: usize) -> Vec<String> {
        (0..n).map(|i| format!("creator{i}")).collect()
    }

    #[test]
    fn selects_the_chunk_at_the_given_offset() {
        let all = slugs(130);
        let shard = ChatShard { index: 1, size: 50 };
        let got = shard.select(&all);
        assert_eq!(got.len(), 50);
        assert!(got.contains("creator50"));
        assert!(got.contains("creator99"));
        assert!(!got.contains("creator49"));
        assert!(!got.contains("creator100"));
    }

    #[test]
    fn last_chunk_may_be_partial() {
        let all = slugs(130);
        let shard = ChatShard { index: 2, size: 50 };
        let got = shard.select(&all);
        assert_eq!(got.len(), 30);
        assert!(got.contains("creator100"));
        assert!(got.contains("creator129"));
    }

    #[test]
    fn out_of_range_offset_is_empty() {
        let all = slugs(130);
        let shard = ChatShard { index: 3, size: 50 };
        assert!(shard.select(&all).is_empty());
    }

    #[test]
    fn chunks_are_disjoint_and_cover_everyone() {
        let all = slugs(130);
        let size = 50;
        let mut seen = std::collections::HashSet::new();
        for index in 0..3 {
            let shard = ChatShard { index, size };
            for slug in shard.select(&all) {
                assert!(seen.insert(slug), "a slug was assigned to two shards");
            }
        }
        assert_eq!(seen.len(), all.len());
    }

    #[test]
    fn zero_size_is_empty() {
        let all = slugs(10);
        let shard = ChatShard { index: 0, size: 0 };
        assert!(shard.select(&all).is_empty());
    }
}
