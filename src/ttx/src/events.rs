//! Event dispatch infrastructure, ported from `TTX.App.Events`.
//!
//! The concrete events now live with their feature modules (e.g.
//! [`crate::players::events`]); this module keeps only the shared contracts.
//!
//! The C# `IEventDispatcher.Dispatch<T>` is generic; Rust trait objects cannot
//! have generic methods and stay object-safe, so [`EventDispatcher`] instead
//! accepts `&dyn Event`. Each event knows its own type name (mirroring
//! `BaseEvent.Type => GetType().Name`) and can render its wire envelope.
//!
//! Concrete cross-process backplanes implementing these contracts live in the
//! [`postgres`] (`LISTEN`/`NOTIFY`) and [`redis`] (pub/sub) submodules.

use async_trait::async_trait;
use serde_json::Value;

use crate::error::Result;

pub mod postgres;
pub mod redis;

pub trait Event: Send + Sync {
    fn event_type(&self) -> &'static str;

    /// The event's payload as JSON.
    fn payload(&self) -> Value;

    /// The full wire envelope: the payload with a `type` tag merged in.
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

/// Implements [`Event`] for a `Serialize` type whose runtime name is `$name`.
/// Shared by the per-feature `events` modules.
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
