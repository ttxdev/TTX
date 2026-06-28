use async_trait::async_trait;
use serde_json::Value;

use crate::error::Result;

pub mod postgres;
pub mod redis;

pub trait Event: Send + Sync {
    fn event_type(&self) -> &'static str;

    fn payload(&self) -> Value;

    fn to_value(&self) -> Value {
        let mut value = self.payload();
        if let Value::Object(map) = &mut value {
            map.insert(
                "type".to_string(),
                Value::String(self.event_type().to_string()),
            );
        }
        value
    }
}

#[async_trait]
pub trait EventDispatcher: Send + Sync {
    async fn dispatch(&self, event: &dyn Event) -> Result<()>;
}

#[async_trait]
pub trait EventReceiver: Send + Sync {
    async fn subscribe(&self, event_type: &'static str) -> tokio::sync::mpsc::Receiver<Value>;
}

macro_rules! impl_event {
    ($ty:ty, $name:literal) => {
        impl $crate::events::Event for $ty {
            fn event_type(&self) -> &'static str {
                $name
            }

            fn payload(&self) -> ::serde_json::Value {
                ::serde_json::to_value(self).unwrap_or(::serde_json::Value::Null)
            }
        }
    };
}

pub(crate) use impl_event;
