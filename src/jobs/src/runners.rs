use std::collections::HashMap;
use std::sync::Arc;

use tokio::task::JoinHandle;
use ttx::events::EventReceiver;
use ttx::events::postgres::PgEventReceiver;
use ttx::factories::ChatMonitorFactory;
use ttx::jobs::chat::twitch::TwitchChatAdapter;
use ttx::jobs::streams::twitch::TwitchStreamMonitorAdapter;
use ttx::jobs::{
    CalculatePlayerPortfolioJob, CalculatePlayerPortfolioOptions, ChatMonitorAdapter,
    CreatorValueMonitorJob, RedisCreatorStatsRepository, StreamMonitorAdapter, StreamMonitorJob,
};
use ttx::platforms::Platform;

use crate::Plugins;
use crate::config;
use crate::context::Context;

type Handle = JoinHandle<()>;

/// Player-portfolio recomputation. Always enabled.
pub fn spawn_portfolio(ctx: &Context) -> Handle {
    let job = CalculatePlayerPortfolioJob::new(
        CalculatePlayerPortfolioOptions::default(),
        ctx.db.clone(),
        ctx.dispatcher.clone(),
    );
    let cancel = ctx.cancel.clone();
    tokio::spawn(async move { job.run(cancel).await })
}

/// Twitch stream-status monitor. Enabled when Twitch credentials are present.
pub fn spawn_stream_monitor(ctx: &Context) -> Option<Handle> {
    let Some(options) = config::twitch_options() else {
        tracing::info!(
            "StreamMonitorJob disabled: set TWITCH_CLIENT_ID and TWITCH_CLIENT_SECRET to enable"
        );
        return None;
    };

    let adapter: Arc<dyn StreamMonitorAdapter> = Arc::new(TwitchStreamMonitorAdapter::new(options));
    let job = StreamMonitorJob::new(ctx.db.clone(), ctx.dispatcher.clone(), vec![adapter]);
    let cancel = ctx.cancel.clone();
    let handle = tokio::spawn(async move {
        if let Err(err) = job.run(cancel).await {
            tracing::error!(?err, "stream monitor stopped");
        }
    });
    tracing::info!("StreamMonitorJob started (Twitch)");
    Some(handle)
}

/// Creator-value monitor (Twitch chat + Redis stats). Enabled when `REDIS_URL`
/// is set and reachable.
pub async fn spawn_creator_values(ctx: &Context, plugins: &dyn Plugins) -> Option<Handle> {
    let Some(redis_url) = config::redis_url() else {
        tracing::info!("CreatorValueMonitorJob disabled: set REDIS_URL to enable");
        return None;
    };
    let stats = match RedisCreatorStatsRepository::connect(&redis_url).await {
        Ok(stats) => stats,
        Err(err) => {
            tracing::error!(?err, "CreatorValueMonitorJob disabled: Redis unavailable");
            return None;
        }
    };

    let chat: Arc<dyn ChatMonitorAdapter> = Arc::new(TwitchChatAdapter::new());
    let mut adapters = HashMap::new();
    adapters.insert(Platform::Twitch, chat);
    let factory = Arc::new(ChatMonitorFactory::new(adapters));

    let receiver: Arc<dyn EventReceiver> = Arc::new(PgEventReceiver::new(ctx.database_url.clone()));

    let job = CreatorValueMonitorJob::new(
        config::creator_values_options(),
        ctx.db.clone(),
        factory,
        plugins.analyzer(),
        Arc::new(stats),
        plugins.processor(&redis_url).await,
        ctx.dispatcher.clone(),
        receiver,
    );
    let cancel = ctx.cancel.clone();
    let handle = tokio::spawn(async move {
        if let Err(err) = job.run(cancel).await {
            tracing::error!(?err, "creator-value monitor stopped");
        }
    });
    tracing::info!("CreatorValueMonitorJob started (Redis + Twitch chat)");
    Some(handle)
}
