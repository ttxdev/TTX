use std::collections::HashMap;
use std::sync::Arc;

use chrono::Duration;
use sqlx::postgres::PgPoolOptions;
use tokio_util::sync::CancellationToken;
use tracing_subscriber::EnvFilter;
use ttx::data::Db;
use ttx::events::postgres::{PgEventReceiver, PgNotifyDispatcher};
use ttx::events::{EventDispatcher, EventReceiver};
use ttx::factories::ChatMonitorFactory;
use ttx::jobs::chat::twitch::TwitchChatAdapter;
use ttx::jobs::streams::twitch::TwitchStreamMonitorAdapter;
use ttx::jobs::{
    CalculatePlayerPortfolioJob, CalculatePlayerPortfolioOptions, ChatMonitorAdapter,
    CreatorValueMonitorJob, CreatorValuesJobOptions, MessageAnalyzer, RedisCreatorStatsRepository,
    StatsProcessor, StreamMonitorAdapter, StreamMonitorJob,
};
use ttx::platforms::Platform;
use ttx::platforms::twitch::TwitchOAuthOptions;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    dotenvy::dotenv().ok();
    tracing_subscriber::fmt()
        .with_env_filter(
            EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info")),
        )
        .init();

    let database_url = std::env::var("DATABASE_URL")
        .unwrap_or_else(|_| "postgres://postgres:postgres@localhost:5432/ttx".into());
    let pool = PgPoolOptions::new()
        .max_connections(5)
        .connect(&database_url)
        .await?;
    sqlx::migrate!("../ttx/migrations").run(&pool).await?;

    // Shared L2 cache: Redis when REDIS_URL is set, else in-process.
    let cache = ttx::cache::cache_from_env().await;
    let db = Db::with_cache(pool.clone(), cache);
    let dispatcher: Arc<dyn EventDispatcher> = Arc::new(PgNotifyDispatcher::new(pool));

    let cancel = CancellationToken::new();
    let mut handles = Vec::new();

    // --- portfolio snapshots (always) ---
    {
        let job = CalculatePlayerPortfolioJob::new(
            CalculatePlayerPortfolioOptions::default(),
            db.clone(),
            dispatcher.clone(),
        );
        let child = cancel.clone();
        handles.push(tokio::spawn(async move { job.run(child).await }));
    }

    // --- stream monitor (Twitch Helix; needs credentials) ---
    match twitch_options() {
        Some(options) => {
            let adapter: Arc<dyn StreamMonitorAdapter> =
                Arc::new(TwitchStreamMonitorAdapter::new(options));
            let job = StreamMonitorJob::new(db.clone(), dispatcher.clone(), vec![adapter]);
            let child = cancel.clone();
            handles.push(tokio::spawn(async move {
                if let Err(err) = job.run(child).await {
                    tracing::error!(?err, "stream monitor stopped");
                }
            }));
            tracing::info!("StreamMonitorJob started (Twitch)");
        }
        None => tracing::info!(
            "StreamMonitorJob disabled: set TWITCH_CLIENT_ID and TWITCH_CLIENT_SECRET to enable"
        ),
    }

    // --- creator-value monitor (Redis-backed stats + anonymous Twitch chat) ---
    match std::env::var("REDIS_URL") {
        Ok(redis_url) => match RedisCreatorStatsRepository::connect(&redis_url).await {
            Ok(stats) => {
                let chat: Arc<dyn ChatMonitorAdapter> = Arc::new(TwitchChatAdapter::new());
                let mut adapters = HashMap::new();
                adapters.insert(Platform::Twitch, chat);
                let factory = Arc::new(ChatMonitorFactory::new(adapters));

                let receiver: Arc<dyn EventReceiver> =
                    Arc::new(PgEventReceiver::new(database_url.clone()));

                // The analyzer and value processor come from `TTX.Private` when
                // the `private` feature is compiled in, otherwise from the
                // open-source fallbacks in `ttx::jobs::backends`.
                let analyzer = make_analyzer();
                let processor = make_processor(&redis_url).await;

                let job = CreatorValueMonitorJob::new(
                    creator_values_options(),
                    db.clone(),
                    factory,
                    analyzer,
                    Arc::new(stats),
                    processor,
                    dispatcher.clone(),
                    receiver,
                );
                let child = cancel.clone();
                handles.push(tokio::spawn(async move {
                    if let Err(err) = job.run(child).await {
                        tracing::error!(?err, "creator-value monitor stopped");
                    }
                }));
                tracing::info!(
                    private = cfg!(feature = "private"),
                    "CreatorValueMonitorJob started (Redis + Twitch chat)"
                );
            }
            Err(err) => {
                tracing::error!(?err, "CreatorValueMonitorJob disabled: Redis unavailable")
            }
        },
        Err(_) => {
            tracing::info!("CreatorValueMonitorJob disabled: set REDIS_URL to enable")
        }
    }

    // Run until Ctrl-C, then cancel and let the jobs wind down.
    tokio::signal::ctrl_c().await?;
    tracing::info!("shutting down jobs runner");
    cancel.cancel();
    for handle in handles {
        let _ = handle.await;
    }

    Ok(())
}

/// Twitch OAuth credentials from the environment (the redirect URI is only used
/// for the API's login flow, so it defaults to empty here).
fn twitch_options() -> Option<TwitchOAuthOptions> {
    Some(TwitchOAuthOptions {
        client_id: std::env::var("TWITCH_CLIENT_ID").ok()?,
        client_secret: std::env::var("TWITCH_CLIENT_SECRET").ok()?,
        redirect_uri: std::env::var("TWITCH_REDIRECT_URI").unwrap_or_default(),
    })
}

/// The message analyzer: VADER (from `TTX.Private`) when the `private` feature
/// is enabled, otherwise the open-source keyword scorer.
#[cfg(feature = "private")]
fn make_analyzer() -> Arc<dyn MessageAnalyzer> {
    Arc::new(private::VaderMessageAnalyzer::new())
}

#[cfg(not(feature = "private"))]
fn make_analyzer() -> Arc<dyn MessageAnalyzer> {
    Arc::new(ttx::jobs::KeywordMessageAnalyzer)
}

/// The value processor: the baseline-EMA algorithm (from `TTX.Private`, backed
/// by a Redis baseline store) when the `private` feature is enabled, otherwise
/// the open-source sum processor. Falls back to the sum processor if the
/// baseline store can't be reached.
#[cfg(feature = "private")]
async fn make_processor(redis_url: &str) -> Arc<dyn StatsProcessor> {
    match private::RedisCreatorBaselineRepository::connect(redis_url).await {
        Ok(baselines) => Arc::new(private::BaselineStatsProcessor::new(Arc::new(baselines))),
        Err(err) => {
            tracing::error!(?err, "baseline store unavailable, using sum processor");
            Arc::new(ttx::jobs::SumStatsProcessor)
        }
    }
}

#[cfg(not(feature = "private"))]
async fn make_processor(_redis_url: &str) -> Arc<dyn StatsProcessor> {
    Arc::new(ttx::jobs::SumStatsProcessor)
}

fn creator_values_options() -> CreatorValuesJobOptions {
    let secs = std::env::var("CREATOR_VALUES_DELAY_SECS")
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(15);
    CreatorValuesJobOptions {
        delay: Duration::seconds(secs),
    }
}
