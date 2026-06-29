use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use std::time::{Duration, Instant};

use async_trait::async_trait;
use moka::Expiry;
use moka::future::Cache as MokaCache;

pub mod lock;
pub mod redis;

pub use lock::LockGuard;
pub use redis::{RedisCache, cache_from_env};

const DEFAULT_CAPACITY: u64 = 10_000;

#[async_trait]
pub trait Cache: Send + Sync {
    async fn get(&self, key: &str) -> Option<String>;
    async fn set(&self, key: &str, value: String, ttl: Duration);

    /// Try once to acquire the lock `key` for `token`, expiring after `ttl`.
    /// Returns `true` if this caller now holds the lock.
    async fn lock(&self, key: &str, token: &str, ttl: Duration) -> bool;

    /// Release the lock `key`, but only if it is still held by `token`.
    async fn unlock(&self, key: &str, token: &str);
}

#[derive(Clone)]
pub struct InMemoryCache {
    inner: MokaCache<String, (String, Duration)>,
    locks: Arc<Mutex<HashMap<String, (String, Instant)>>>,
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
            locks: Arc::new(Mutex::new(HashMap::new())),
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

    async fn lock(&self, key: &str, token: &str, ttl: Duration) -> bool {
        let now = Instant::now();
        let mut locks = self.locks.lock().expect("lock map poisoned");
        match locks.get(key) {
            Some((_, expiry)) if *expiry > now => false,
            _ => {
                locks.insert(key.to_string(), (token.to_string(), now + ttl));
                true
            }
        }
    }

    async fn unlock(&self, key: &str, token: &str) {
        let mut locks = self.locks.lock().expect("lock map poisoned");
        if locks.get(key).is_some_and(|(held, _)| held == token) {
            locks.remove(key);
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[tokio::test]
    async fn held_lock_blocks_until_released() {
        let cache = InMemoryCache::new();
        assert!(cache.lock("k", "a", Duration::from_secs(5)).await);
        assert!(!cache.lock("k", "b", Duration::from_secs(5)).await);
        cache.unlock("k", "a").await;
        assert!(cache.lock("k", "b", Duration::from_secs(5)).await);
    }

    #[tokio::test]
    async fn unlock_with_wrong_token_is_a_noop() {
        let cache = InMemoryCache::new();
        assert!(cache.lock("k", "owner", Duration::from_secs(5)).await);
        cache.unlock("k", "intruder").await;
        assert!(!cache.lock("k", "other", Duration::from_secs(5)).await);
    }

    #[tokio::test]
    async fn expired_lock_can_be_reacquired() {
        let cache = InMemoryCache::new();
        assert!(cache.lock("k", "a", Duration::from_millis(20)).await);
        assert!(!cache.lock("k", "b", Duration::from_secs(5)).await);
        tokio::time::sleep(Duration::from_millis(40)).await;
        assert!(cache.lock("k", "b", Duration::from_secs(5)).await);
    }
}
