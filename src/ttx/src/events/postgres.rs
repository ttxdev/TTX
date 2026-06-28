//! Cross-process event delivery over Postgres `LISTEN`/`NOTIFY`.
//!
//! The C# stack fanned events out to all instances through a Redis SignalR
//! backplane. Here, a separate process (e.g. the jobs runner) dispatches events
//! with [`PgNotifyDispatcher`], which `NOTIFY`s a channel; subscribers — the
//! API's websocket layer — pick them up via [`listen`] and rebroadcast locally.
//!
//! Payloads must stay under Postgres's ~8 KB `NOTIFY` limit. The events sent
//! this way (portfolio/value/stream updates) are small; larger in-process
//! events should be dispatched locally rather than through here.

use async_trait::async_trait;
use serde_json::Value;
use sqlx::PgPool;
use sqlx::postgres::PgListener;
use tokio::sync::mpsc;

use crate::error::{Error, Result};
use crate::events::{Event, EventDispatcher, EventReceiver};

/// The `NOTIFY` channel carrying serialized event envelopes.
pub const EVENT_CHANNEL: &str = "ttx_events";

/// An [`EventDispatcher`] that broadcasts each event to all listeners via
/// `pg_notify`.
pub struct PgNotifyDispatcher {
    pool: PgPool,
}

impl PgNotifyDispatcher {
    pub fn new(pool: PgPool) -> Self {
        Self { pool }
    }
}

#[async_trait]
impl EventDispatcher for PgNotifyDispatcher {
    async fn dispatch(&self, event: &dyn Event) -> Result<()> {
        let payload = event.to_value().to_string();
        sqlx::query("SELECT pg_notify($1, $2)")
            .bind(EVENT_CHANNEL)
            .bind(payload)
            .execute(&self.pool)
            .await?;
        Ok(())
    }
}

/// Subscribes to [`EVENT_CHANNEL`] and returns a channel of decoded event
/// envelopes. A background task forwards notifications until the receiver is
/// dropped. `sqlx`'s `PgListener` reconnects transparently.
pub async fn listen(database_url: &str) -> Result<mpsc::Receiver<Value>> {
    let mut listener = PgListener::connect(database_url)
        .await
        .map_err(|e| Error::Database(e.to_string()))?;
    listener
        .listen(EVENT_CHANNEL)
        .await
        .map_err(|e| Error::Database(e.to_string()))?;

    let (tx, rx) = mpsc::channel(256);
    tokio::spawn(async move {
        loop {
            match listener.recv().await {
                Ok(notification) => {
                    if let Ok(value) = serde_json::from_str::<Value>(notification.payload())
                        && tx.send(value).await.is_err()
                    {
                        break; // receiver dropped
                    }
                }
                Err(err) => {
                    tracing::error!(?err, "postgres event listener stopped");
                    break;
                }
            }
        }
    });

    Ok(rx)
}

/// An [`EventReceiver`] over the same Postgres channel, filtered by event type.
/// Lets consumers (e.g. the creator-value job) subscribe to a single event kind
/// on the backplane the API also uses.
pub struct PgEventReceiver {
    database_url: String,
}

impl PgEventReceiver {
    pub fn new(database_url: String) -> Self {
        Self { database_url }
    }
}

#[async_trait]
impl EventReceiver for PgEventReceiver {
    async fn subscribe(&self, event_type: &'static str) -> mpsc::Receiver<Value> {
        let (tx, rx) = mpsc::channel(64);
        let url = self.database_url.clone();
        tokio::spawn(async move {
            let mut all = match listen(&url).await {
                Ok(all) => all,
                Err(err) => {
                    tracing::error!(?err, "could not subscribe to the Postgres event channel");
                    return;
                }
            };
            while let Some(value) = all.recv().await {
                let matches = value.get("type").and_then(|t| t.as_str()) == Some(event_type);
                if matches && tx.send(value).await.is_err() {
                    break;
                }
            }
        });
        rx
    }
}
