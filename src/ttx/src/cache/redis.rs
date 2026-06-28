//! Redis-backed [`Cache`] implementation (ported from `TTX.Infrastructure`).
//!
//! The shared, out-of-process counterpart to [`InMemoryCache`]: use it in
//! production so multiple API instances (and the jobs runner) share one cache
//! rather than each warming its own.

use std::sync::Arc;
use std::time::Duration;

use async_trait::async_trait;
use redis::AsyncCommands;
use redis::aio::ConnectionManager;

use crate::cache::{Cache, InMemoryCache};
use crate::error::{Error, Result};

/// Namespaces our cache keys inside the Redis keyspace so they don't collide
/// with other data (e.g. `creator_stats`).
const KEY_PREFIX: &str = "ttx:cache:";

fn ext(e: impl std::fmt::Display) -> Error {
    Error::External(e.to_string())
}

/// Redis-backed [`Cache`]. Uses `SET key value EX ttl` / `GET key`; per-key TTL
/// is native to Redis so entries expire on their own schedule.
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
        // Redis TTLs are whole seconds; never set 0 (which would be an error).
        let secs = ttl.as_secs().max(1);
        if let Err(err) = conn.set_ex::<_, _, ()>(&namespaced, value, secs).await {
            tracing::warn!(%key, error = %err, "redis cache set failed");
        }
    }
}

/// Builds the cache backend from the environment: a shared [`RedisCache`] when
/// `REDIS_URL` is set (and reachable), otherwise the in-process fallback. This
/// is the single wiring point shared by the API and jobs binaries.
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
