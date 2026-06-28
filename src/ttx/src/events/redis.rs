use async_trait::async_trait;
use futures_util::StreamExt;
use redis::AsyncCommands;
use redis::aio::ConnectionManager;
use serde_json::Value;
use tokio::sync::mpsc;

use crate::error::{Error, Result};
use crate::events::{Event, EventDispatcher, EventReceiver};

const EVENT_CHANNEL: &str = "TTX.Events";

fn ext(e: impl std::fmt::Display) -> Error {
    Error::External(e.to_string())
}

#[derive(Clone)]
pub struct RedisEventDispatcher {
    conn: ConnectionManager,
}

impl RedisEventDispatcher {
    pub async fn connect(url: &str) -> Result<Self> {
        let client = redis::Client::open(url).map_err(ext)?;
        let conn = ConnectionManager::new(client).await.map_err(ext)?;
        Ok(Self { conn })
    }
}

#[async_trait]
impl EventDispatcher for RedisEventDispatcher {
    async fn dispatch(&self, event: &dyn Event) -> Result<()> {
        let mut conn = self.conn.clone();
        let payload = event.to_value().to_string();
        let _: () = conn.publish(EVENT_CHANNEL, payload).await.map_err(ext)?;
        Ok(())
    }
}

pub struct RedisEventReceiver {
    client: redis::Client,
}

impl RedisEventReceiver {
    pub fn new(url: &str) -> Result<Self> {
        Ok(Self {
            client: redis::Client::open(url).map_err(ext)?,
        })
    }
}

#[async_trait]
impl EventReceiver for RedisEventReceiver {
    async fn subscribe(&self, event_type: &'static str) -> mpsc::Receiver<Value> {
        let (tx, rx) = mpsc::channel(64);
        let client = self.client.clone();
        tokio::spawn(async move {
            let mut pubsub = match client.get_async_pubsub().await {
                Ok(pubsub) => pubsub,
                Err(err) => {
                    tracing::error!(%err, "redis pubsub connect failed");
                    return;
                }
            };
            if let Err(err) = pubsub.subscribe(EVENT_CHANNEL).await {
                tracing::error!(%err, "redis subscribe failed");
                return;
            }

            let mut stream = pubsub.on_message();
            while let Some(msg) = stream.next().await {
                let Ok(payload) = msg.get_payload::<String>() else {
                    continue;
                };
                if let Ok(value) = serde_json::from_str::<Value>(&payload) {
                    let matches = value.get("type").and_then(|t| t.as_str()) == Some(event_type);
                    if matches && tx.send(value).await.is_err() {
                        break;
                    }
                }
            }
        });
        rx
    }
}
