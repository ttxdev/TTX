mod config;
mod context;
mod deps;
mod runners;

use context::Context;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    dotenvy::dotenv().ok();
    config::init_tracing();

    let ctx = Context::init().await?;

    let mut handles = vec![runners::spawn_portfolio(&ctx)];
    handles.extend(runners::spawn_stream_monitor(&ctx));
    handles.extend(runners::spawn_creator_values(&ctx).await);

    tokio::signal::ctrl_c().await?;
    tracing::info!("shutting down jobs runner");
    ctx.cancel.cancel();
    for handle in handles {
        let _ = handle.await;
    }

    Ok(())
}
