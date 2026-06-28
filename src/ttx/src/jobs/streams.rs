pub mod twitch;

use std::sync::Arc;
use std::time::Duration;

use async_trait::async_trait;
use chrono::{DateTime, Utc};
use tokio::sync::mpsc;
use tokio_util::sync::CancellationToken;

use crate::creators::Creator;
use crate::creators::events::UpdateStreamStatusEvent;
use crate::data::Db;
use crate::error::Result;
use crate::events::EventDispatcher;
use crate::primitives::Id;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct StreamUpdateEvent {
    pub creator_id: Id,
    pub is_live: bool,
    pub at: DateTime<Utc>,
}

#[async_trait]
pub trait StreamMonitorAdapter: Send + Sync {
    async fn start(
        &self,
        sink: mpsc::Sender<StreamUpdateEvent>,
        cancel: CancellationToken,
    ) -> Result<()>;
    fn set_creators(&self, creators: Vec<Creator>);
    fn remove_creator(&self, creator_id: Id) -> bool;
}

pub struct StreamMonitorJob {
    db: Db,
    events: Arc<dyn EventDispatcher>,
    adapters: Vec<Arc<dyn StreamMonitorAdapter>>,
}

impl StreamMonitorJob {
    pub fn new(
        db: Db,
        events: Arc<dyn EventDispatcher>,
        adapters: Vec<Arc<dyn StreamMonitorAdapter>>,
    ) -> Self {
        Self {
            db,
            events,
            adapters,
        }
    }

    pub async fn run(&self, cancel: CancellationToken) -> Result<()> {
        if self.adapters.is_empty() {
            tracing::warn!("No stream monitor adapters found");
            return Ok(());
        }

        // Mark everyone offline, then prime the adapters with the roster.
        self.db.reset_all_streams_offline().await?;
        let creators = self.db.all_creators().await?;
        tracing::info!(
            "Monitoring {}",
            creators
                .iter()
                .map(|c| c.user.name.as_str())
                .collect::<Vec<_>>()
                .join(" ")
        );

        let (tx, mut rx) = mpsc::channel::<StreamUpdateEvent>(256);
        for adapter in &self.adapters {
            adapter.set_creators(creators.clone());
            let adapter = adapter.clone();
            let sink = tx.clone();
            let child = cancel.clone();
            tokio::spawn(async move {
                if let Err(err) = adapter.start(sink, child).await {
                    tracing::error!(?err, "Failed to get streams");
                }
            });
        }
        drop(tx);

        loop {
            tokio::select! {
                _ = cancel.cancelled() => break,
                event = rx.recv() => {
                    match event {
                        Some(event) => {
                            if let Err(err) = self.apply(event).await {
                                tracing::error!(?err, "Failed to apply stream update");
                            }
                        }
                        None => break,
                    }
                }
                _ = tokio::time::sleep(Duration::from_millis(300)) => {}
            }
        }

        Ok(())
    }

    async fn apply(&self, event: StreamUpdateEvent) -> Result<()> {
        let Some(mut creator) = self.find_creator(event.creator_id).await? else {
            for adapter in &self.adapters {
                adapter.remove_creator(event.creator_id);
            }
            return Ok(());
        };

        if event.is_live {
            creator.stream_status.started(event.at);
            tracing::info!(
                "{} is now live on {:?}",
                creator.user.name,
                creator.user.platform
            );
        } else {
            creator.stream_status.ended(event.at);
            tracing::info!(
                "{} ended stream on {:?}",
                creator.user.name,
                creator.user.platform
            );
        }

        self.db.update_stream_status(&creator).await?;
        self.events
            .dispatch(&UpdateStreamStatusEvent::create(&creator))
            .await?;
        Ok(())
    }

    async fn find_creator(&self, creator_id: Id) -> Result<Option<Creator>> {
        Ok(self
            .db
            .creators_by_ids(&[creator_id])
            .await?
            .into_iter()
            .next())
    }
}
