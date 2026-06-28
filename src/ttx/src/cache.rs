use std::time::{Duration, Instant};

use async_trait::async_trait;
use moka::Expiry;
use moka::future::Cache as MokaCache;

pub mod redis;

pub use redis::{RedisCache, cache_from_env};

const DEFAULT_CAPACITY: u64 = 10_000;

#[async_trait]
pub trait Cache: Send + Sync {
    async fn get(&self, key: &str) -> Option<String>;
    async fn set(&self, key: &str, value: String, ttl: Duration);
}

#[derive(Clone)]
pub struct InMemoryCache {
    inner: MokaCache<String, (String, Duration)>,
}

struct PerEntryTtl;

impl Expiry<String, (String, Duration)> for PerEntryTtl {
    fn expire_after_create(
        &self,
        _key: &String,
        value: &(String, Duration),
        _created_at: Instant,
    ) -> Option<Duration> {
        Some(value.1)
    }

    fn expire_after_update(
        &self,
        _key: &String,
        value: &(String, Duration),
        _updated_at: Instant,
        _current: Option<Duration>,
    ) -> Option<Duration> {
        Some(value.1)
    }
}

impl InMemoryCache {
    pub fn new() -> Self {
        Self::with_capacity(DEFAULT_CAPACITY)
    }

    pub fn with_capacity(capacity: u64) -> Self {
        Self {
            inner: MokaCache::builder()
                .max_capacity(capacity)
                .expire_after(PerEntryTtl)
                .build(),
        }
    }
}

impl Default for InMemoryCache {
    fn default() -> Self {
        Self::new()
    }
}

#[async_trait]
impl Cache for InMemoryCache {
    async fn get(&self, key: &str) -> Option<String> {
        self.inner.get(key).await.map(|(value, _)| value)
    }

    async fn set(&self, key: &str, value: String, ttl: Duration) {
        self.inner.insert(key.to_string(), (value, ttl)).await;
    }
}
