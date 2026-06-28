use std::sync::Arc;
use std::time::Duration;

use async_trait::async_trait;
use redis::AsyncCommands;
use redis::aio::ConnectionManager;

use crate::cache::{Cache, InMemoryCache};
use crate::error::{Error, Result};

const KEY_PREFIX: &str = "ttx:cache:";

fn ext(e: impl std::fmt::Display) -> Error {
    Error::External(e.to_string())
}

#[derive(Clone)]
pub struct RedisCache {
    conn: ConnectionManager,
}

impl RedisCache {
    pub async fn connect(url: &str) -> Result<Self> {
        let client = redis::Client::open(url).map_err(ext)?;
        let conn = ConnectionManager::new(client).await.map_err(ext)?;
        Ok(Self { conn })
    }
}

#[async_trait]
impl Cache for RedisCache {
    async fn get(&self, key: &str) -> Option<String> {
        let mut conn = self.conn.clone();
        let namespaced = format!("{KEY_PREFIX}{key}");
        match conn.get::<_, Option<String>>(&namespaced).await {
            Ok(value) => value,
            Err(err) => {
                tracing::warn!(%key, error = %err, "redis cache get failed");
                None
            }
        }
    }

    async fn set(&self, key: &str, value: String, ttl: Duration) {
        let mut conn = self.conn.clone();
        let namespaced = format!("{KEY_PREFIX}{key}");
        let secs = ttl.as_secs().max(1);
        if let Err(err) = conn.set_ex::<_, _, ()>(&namespaced, value, secs).await {
            tracing::warn!(%key, error = %err, "redis cache set failed");
        }
    }
}

pub async fn cache_from_env() -> Arc<dyn Cache> {
    match std::env::var("REDIS_URL") {
        Ok(url) => match RedisCache::connect(&url).await {
            Ok(cache) => {
                tracing::info!("cache: Redis (shared)");
                Arc::new(cache)
            }
            Err(err) => {
                tracing::warn!(error = %err, "cache: Redis unavailable, using in-memory fallback");
                Arc::new(InMemoryCache::new())
            }
        },
        Err(_) => {
            tracing::info!("cache: in-memory (set REDIS_URL for a shared cache)");
            Arc::new(InMemoryCache::new())
        }
    }
}
