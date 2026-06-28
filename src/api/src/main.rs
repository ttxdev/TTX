mod auth;
mod error;
mod events;
mod routes;
mod seed;
mod state;

use std::collections::HashMap;
use std::sync::Arc;

use sqlx::postgres::PgPoolOptions;
use tracing_subscriber::EnvFilter;
use ttx::di::Ttx;
use ttx::events::EventDispatcher;
use ttx::jobs::ChatMonitorAdapter;
use ttx::options::RandomOptions;
use ttx::platforms::Platform;
use ttx::platforms::PlatformUserService;

use auth::JwtConfig;
use events::EventBroadcaster;
use state::AppState;
use ttx::platforms::twitch::{TwitchOAuthOptions, TwitchUserService};

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    // Load `.env` before reading any configuration (so `RUST_LOG` applies to the
    // subscriber below). Missing file is fine — real env vars still win.
    dotenvy::dotenv().ok();

    tracing_subscriber::fmt()
        .with_env_filter(
            EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info")),
        )
        .init();

    // --- database ---
    let database_url = std::env::var("DATABASE_URL")
        .unwrap_or_else(|_| "postgres://postgres:postgres@localhost/ttx".into());
    let pool = PgPoolOptions::new()
        .max_connections(10)
        .connect(&database_url)
        .await?;
    sqlx::migrate!("../ttx/migrations").run(&pool).await?;

    // --- shared collaborators ---
    let events = EventBroadcaster::new();
    let dispatcher: Arc<dyn EventDispatcher> = Arc::new(events.clone());

    // Real Twitch Helix/OAuth service (ported from TTX.Infrastructure). The
    // login URL is derived from the client id + redirect URI.
    let twitch_options = TwitchOAuthOptions {
        client_id: std::env::var("TWITCH_CLIENT_ID").unwrap_or_default(),
        client_secret: std::env::var("TWITCH_CLIENT_SECRET").unwrap_or_default(),
        redirect_uri: std::env::var("TWITCH_REDIRECT_URI").unwrap_or_default(),
    };
    let twitch: Arc<dyn PlatformUserService> = Arc::new(TwitchUserService::new(twitch_options));

    let mut platform_users: HashMap<Platform, Arc<dyn PlatformUserService>> = HashMap::new();
    platform_users.insert(Platform::Twitch, twitch.clone());
    let chat_adapters: HashMap<Platform, Arc<dyn ChatMonitorAdapter>> = HashMap::new();

    // Shared L2 cache: Redis when REDIS_URL is set, else in-process.
    let cache = ttx::cache::cache_from_env().await;

    let ttx = Ttx::new(
        pool,
        dispatcher,
        platform_users,
        chat_adapters,
        RandomOptions::default(),
        cache,
    );

    let state = AppState {
        ttx: Arc::new(ttx),
        jwt: Arc::new(JwtConfig::from_env()),
        events,
        twitch,
    };

    if std::env::args().skip(1).any(|arg| arg == "seed") {
        seed::seed(&state.ttx.db).await?;
        return Ok(());
    }

    // --- relay cross-process events to websocket clients ---
    // The jobs runner dispatches via Postgres NOTIFY; subscribe and fan those
    // events out locally (the faithful analog of the C# Redis SignalR backplane).
    match ttx::events::postgres::listen(&database_url).await {
        Ok(mut rx) => {
            let broadcaster = state.events.clone();
            tokio::spawn(async move {
                while let Some(value) = rx.recv().await {
                    broadcaster.publish(value);
                }
            });
        }
        Err(err) => tracing::warn!(?err, "could not subscribe to the Postgres event channel"),
    }

    // --- serve ---
    let bind_addr = std::env::var("BIND_ADDR").unwrap_or_else(|_| "0.0.0.0:3000".into());
    let listener = tokio::net::TcpListener::bind(&bind_addr).await?;
    tracing::info!("listening on {bind_addr}");
    axum::serve(listener, routes::router(state)).await?;

    Ok(())
}
