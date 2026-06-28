//! Pluggable cache for the expensive read paths (gap-filled history series and
//! the creator-value average; see `docs/caching.md`).
//!
//! The cache is abstracted behind the [`Cache`] trait so the backend can be
//! swapped without touching the data layer. Two implementations exist:
//!
//!   * [`InMemoryCache`] — an in-process `moka` cache. The default; fine for a
//!     single instance and for tests/dev with no Redis.
//!   * [`RedisCache`] — a shared, out-of-process cache. Use it in production so
//!     multiple API instances (and the jobs runner) share one cache rather than
//!     each warming its own.
//!
//! Values are opaque JSON strings with a per-entry TTL; the data layer
//! serializes/deserializes its domain types around this interface.

use std::time::{Duration, Instant};

use async_trait::async_trait;
use moka::Expiry;
use moka::future::Cache as MokaCache;

pub mod redis;

pub use redis::{RedisCache, cache_from_env};

/// Default capacity for the in-memory backend (entries, not bytes).
const DEFAULT_CAPACITY: u64 = 10_000;

/// A best-effort, TTL'd string cache. Misses (and backend errors) simply mean
/// the caller recomputes, so implementations should never surface errors.
#[async_trait]
pub trait Cache: Send + Sync {
    /// Returns the cached value for `key`, or `None` on miss/expiry/error.
    async fn get(&self, key: &str) -> Option<String>;
    /// Stores `value` under `key`, expiring after `ttl`.
    async fn set(&self, key: &str, value: String, ttl: Duration);
}

/// In-process [`Cache`] backed by `moka`. Per-entry TTLs are honored via an
/// [`Expiry`] that reads the TTL stored alongside each value.
#[derive(Clone)]
pub struct InMemoryCache {
    inner: MokaCache<String, (String, Duration)>,
}

/// Reads the per-entry TTL out of the stored `(value, ttl)` tuple so each key
/// can expire on its own schedule (history vs. average use different TTLs).
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
