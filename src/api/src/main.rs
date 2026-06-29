mod auth;
mod config;
mod error;
mod events;
mod routes;
mod seed;
mod server;
mod state;

use state::AppState;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    dotenvy::dotenv().ok();
    // Held until `main` returns so trace export is flushed on shutdown.
    let _telemetry = config::init_tracing();

    let database_url = config::database_url();
    let state = AppState::build(&database_url).await?;

    if std::env::args().skip(1).any(|arg| arg == "seed") {
        seed::seed(&state.ttx.db).await?;
        return Ok(());
    }

    server::serve(state, &database_url).await
}
