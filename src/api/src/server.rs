use crate::config;
use crate::events::EventBroadcaster;
use crate::routes;
use crate::state::AppState;

pub async fn serve(state: AppState, database_url: &str) -> Result<(), Box<dyn std::error::Error>> {
    spawn_pg_event_listener(state.events.clone(), database_url).await;

    let bind_addr = config::bind_addr();
    let listener = tokio::net::TcpListener::bind(&bind_addr).await?;
    tracing::info!("listening on {bind_addr}");
    axum::serve(listener, routes::router(state)).await?;

    Ok(())
}

/// Bridge Postgres `NOTIFY` events onto the in-process broadcaster (SSE). A
/// failure to subscribe is logged and the server still starts.
async fn spawn_pg_event_listener(events: EventBroadcaster, database_url: &str) {
    match ttx::events::postgres::listen(database_url).await {
        Ok(mut rx) => {
            tokio::spawn(async move {
                while let Some(value) = rx.recv().await {
                    events.publish(value);
                }
            });
        }
        Err(err) => tracing::warn!(?err, "could not subscribe to the Postgres event channel"),
    }
}
