use std::sync::Arc;
use std::time::Duration;

use crate::cache::Cache;

/// How long a lock survives without an explicit release (crash safety-net).
pub const LOCK_TTL: Duration = Duration::from_secs(5);
/// How long an acquirer waits for a contended lock before giving up.
pub const LOCK_WAIT: Duration = Duration::from_secs(2);
/// Delay between acquisition attempts while waiting.
pub const LOCK_BACKOFF: Duration = Duration::from_millis(50);

/// A held distributed lock. Releasing happens on drop (best-effort, spawned);
/// the lock's TTL guarantees eventual release even if the process dies.
pub struct LockGuard {
    cache: Arc<dyn Cache>,
    key: String,
    token: String,
}

impl LockGuard {
    pub(crate) fn new(cache: Arc<dyn Cache>, key: String, token: String) -> Self {
        Self { cache, key, token }
    }
}

impl Drop for LockGuard {
    fn drop(&mut self) {
        let cache = self.cache.clone();
        let key = std::mem::take(&mut self.key);
        let token = std::mem::take(&mut self.token);
        tokio::spawn(async move {
            cache.unlock(&key, &token).await;
        });
    }
}

/// Generate a unique token identifying a single lock holder.
pub(crate) fn new_token() -> String {
    format!("{:032x}", rand::random::<u128>())
}
