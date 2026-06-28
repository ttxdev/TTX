use async_trait::async_trait;
use serde_json::Value;
use sqlx::PgPool;
use sqlx::postgres::PgListener;
use tokio::sync::mpsc;

use crate::error::{Error, Result};
use crate::events::{Event, EventDispatcher, EventReceiver};

pub const EVENT_CHANNEL: &str = "ttx_events";

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
                        break;
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
