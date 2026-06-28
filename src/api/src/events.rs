use std::sync::Arc;

use async_trait::async_trait;
use serde_json::Value;
use tokio::sync::{broadcast, mpsc};
use ttx::events::{Event, EventDispatcher, EventReceiver};

#[derive(Clone)]
pub struct EventBroadcaster {
    tx: broadcast::Sender<Arc<Value>>,
}

impl EventBroadcaster {
    pub fn new() -> Self {
        let (tx, _) = broadcast::channel(256);
        Self { tx }
    }

    pub fn subscribe_raw(&self) -> broadcast::Receiver<Arc<Value>> {
        self.tx.subscribe()
    }

    pub fn publish(&self, value: Value) {
        let _ = self.tx.send(Arc::new(value));
    }
}

impl Default for EventBroadcaster {
    fn default() -> Self {
        Self::new()
    }
}

#[async_trait]
impl EventDispatcher for EventBroadcaster {
    async fn dispatch(&self, event: &dyn Event) -> ttx::error::Result<()> {
        let _ = self.tx.send(Arc::new(event.to_value()));
        Ok(())
    }
}

#[async_trait]
impl EventReceiver for EventBroadcaster {
    async fn subscribe(&self, event_type: &'static str) -> mpsc::Receiver<Value> {
        let (forward_tx, forward_rx) = mpsc::channel(64);
        let mut rx = self.tx.subscribe();
        tokio::spawn(async move {
            while let Ok(value) = rx.recv().await {
                if value.get("type").and_then(|t| t.as_str()) == Some(event_type)
                    && forward_tx.send((*value).clone()).await.is_err()
                {
                    break;
                }
            }
        });
        forward_rx
    }
}
