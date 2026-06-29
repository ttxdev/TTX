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

/// A selectable job runner. Pass one or more names as CLI args to run a subset
/// (e.g. `jobs portfolio streams`); with no args, every job runs.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Job {
    Portfolio,
    Streams,
    Creators,
}

impl Job {
    const ALL: [Job; 3] = [Job::Portfolio, Job::Streams, Job::Creators];

    fn parse(name: &str) -> Option<Job> {
        match name {
            "portfolio" => Some(Job::Portfolio),
            "streams" => Some(Job::Streams),
            "creators" | "creator-values" => Some(Job::Creators),
            _ => None,
        }
    }

    /// Resolve the jobs to run from CLI args, defaulting to all when none are
    /// given. Unknown names are rejected; duplicates are ignored.
    fn select(args: impl Iterator<Item = String>) -> Result<Vec<Job>, String> {
        let mut jobs = Vec::new();
        for arg in args {
            match Job::parse(&arg) {
                Some(job) if !jobs.contains(&job) => jobs.push(job),
                Some(_) => {}
                None => {
                    return Err(format!(
                        "unknown job {arg:?}; valid jobs: portfolio, streams, creators"
                    ));
                }
            }
        }
        if jobs.is_empty() {
            jobs.extend(Job::ALL);
        }
        Ok(jobs)
    }
}

/// Boot the jobs runner: initialise logging, build shared infrastructure, spawn
/// the selected jobs, and run until Ctrl-C.
pub async fn run(plugins: impl Plugins) -> Result<(), Box<dyn Error>> {
    dotenvy::dotenv().ok();
    // Held until `run` returns so trace export is flushed on shutdown.
    let _telemetry = config::init_tracing();

    let jobs = Job::select(std::env::args().skip(1))?;
    tracing::info!(?jobs, "starting jobs runner");

    let ctx = Context::init().await?;

    let mut handles = Vec::new();
    for job in jobs {
        match job {
            Job::Portfolio => handles.push(runners::spawn_portfolio(&ctx)),
            Job::Streams => handles.extend(runners::spawn_stream_monitor(&ctx)),
            Job::Creators => {
                handles.extend(runners::spawn_creator_values(&ctx, &plugins).await)
            }
        }
    }

    tokio::signal::ctrl_c().await?;
    tracing::info!("shutting down jobs runner");
    ctx.cancel.cancel();
    for handle in handles {
        let _ = handle.await;
    }

    Ok(())
}
