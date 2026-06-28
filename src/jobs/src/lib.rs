pub mod config;
pub mod context;
pub mod runners;

use std::error::Error;
use std::sync::Arc;

use async_trait::async_trait;
use ttx::jobs::{MessageAnalyzer, StatsProcessor};

use context::Context;

/// Pluggable implementations the binary injects into the jobs runner. The
/// open-source binary supplies keyword/sum defaults; the closed-source binary
/// (in the `TTX/private` repo) supplies the VADER analyzer and baseline
/// processor. Keeping this as an injected trait is what lets the public
/// workspace build without any reference to the `private` crate.
#[async_trait]
pub trait Plugins: Send + Sync + 'static {
    fn analyzer(&self) -> Arc<dyn MessageAnalyzer>;
    async fn processor(&self, redis_url: &str) -> Arc<dyn StatsProcessor>;
}

/// Boot the jobs runner: initialise logging, build shared infrastructure, spawn
/// each job, and run until Ctrl-C.
pub async fn run(plugins: impl Plugins) -> Result<(), Box<dyn Error>> {
    dotenvy::dotenv().ok();
    config::init_tracing();

    let ctx = Context::init().await?;

    let mut handles = vec![runners::spawn_portfolio(&ctx)];
    handles.extend(runners::spawn_stream_monitor(&ctx));
    handles.extend(runners::spawn_creator_values(&ctx, &plugins).await);

    tokio::signal::ctrl_c().await?;
    tracing::info!("shutting down jobs runner");
    ctx.cancel.cancel();
    for handle in handles {
        let _ = handle.await;
    }

    Ok(())
}
