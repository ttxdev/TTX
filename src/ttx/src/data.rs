pub mod portfolio;

use std::collections::HashMap;
use std::sync::Arc;
use std::time::{Duration as StdDuration, Instant};

use chrono::{DateTime, Duration, Utc};
use serde::Serialize;
use serde::de::DeserializeOwned;
use sqlx::postgres::PgRow;
use sqlx::{PgPool, Row};

use crate::cache::lock::{LOCK_BACKOFF, LOCK_TTL, LOCK_WAIT, new_token};
use crate::cache::{Cache, InMemoryCache, LockGuard};
use crate::creators::{Creator, Vote};
use crate::dto::portfolio::TimeStep;
use crate::error::{Error, Result};
use crate::platforms::Platform;
use crate::players::{Player, PortfolioSnapshot};
use crate::primitives::{Id, Meta};

pub use portfolio::PortfolioRepository;

const HISTORY_TTL: StdDuration = StdDuration::from_secs(15);
pub(crate) const AVERAGE_TTL: StdDuration = StdDuration::from_secs(30);

pub(crate) fn platform_to_str(p: Platform) -> &'static str {
    match p {
        Platform::Twitch => "Twitch",
    }
}

pub(crate) fn parse_platform(s: &str) -> Platform {
    match s {
        "Twitch" => Platform::Twitch,
        _ => Platform::Twitch,
    }
}

pub(crate) fn platform_to_int(p: Platform) -> i32 {
    match p {
        Platform::Twitch => 0,
    }
}

pub(crate) fn col_id(row: &PgRow, name: &str) -> sqlx::Result<Id> {
    Ok(row.try_get::<i32, _>(name)? as Id)
}

pub(crate) fn meta_from(row: &PgRow) -> sqlx::Result<Meta> {
    Ok(Meta {
        id: col_id(row, "id")?,
        created_at: row.try_get::<DateTime<Utc>, _>("created_at")?,
        updated_at: row.try_get::<DateTime<Utc>, _>("updated_at")?,
    })
}

#[derive(Clone)]
pub struct Db {
    pub pool: PgPool,
    cache: Arc<dyn Cache>,
}

impl Db {
    pub fn new(pool: PgPool) -> Self {
        Self::with_cache(pool, Arc::new(InMemoryCache::new()))
    }

    pub fn with_cache(pool: PgPool, cache: Arc<dyn Cache>) -> Self {
        Self { pool, cache }
    }

    pub fn portfolio(&self) -> PortfolioRepository {
        PortfolioRepository::new(self.pool.clone())
    }

    pub(crate) async fn cache_get_json<T: DeserializeOwned>(&self, key: &str) -> Option<T> {
        let raw = self.cache.get(key).await?;
        serde_json::from_str(&raw).ok()
    }

    pub(crate) async fn cache_set_json<T: Serialize>(
        &self,
        key: &str,
        value: &T,
        ttl: StdDuration,
    ) {
        if let Ok(raw) = serde_json::to_string(value) {
            self.cache.set(key, raw, ttl).await;
        }
    }

    fn history_key(kind: &str, id: Id, step: &TimeStep, before: &Duration) -> String {
        format!("hist:{kind}:{id}:{step:?}:{}", before.num_seconds())
    }

    /// Acquire a distributed lock on `key`, waiting up to `wait` for it to free
    /// up. Returns [`Error::Busy`] if it stays contended past the deadline.
    pub(crate) async fn lock(
        &self,
        key: &str,
        ttl: StdDuration,
        wait: StdDuration,
    ) -> Result<LockGuard> {
        let token = new_token();
        let deadline = Instant::now() + wait;
        loop {
            if self.cache.lock(key, &token, ttl).await {
                return Ok(LockGuard::new(self.cache.clone(), key.to_string(), token));
            }
            if Instant::now() >= deadline {
                return Err(Error::Busy(format!("'{key}' is busy, please try again")));
            }
            tokio::time::sleep(LOCK_BACKOFF).await;
        }
    }

    /// Lock a single player's balances for the duration of a read-modify-write.
    pub(crate) async fn lock_player(&self, id: Id) -> Result<LockGuard> {
        self.lock(&format!("player:{id}"), LOCK_TTL, LOCK_WAIT)
            .await
    }

    pub async fn creator_history(
        &self,
        creators: &[Creator],
        step: TimeStep,
        before: Duration,
    ) -> Result<HashMap<Id, Vec<Vote>>> {
        let mut result = HashMap::new();
        let mut misses: Vec<&Creator> = Vec::new();

        for creator in creators {
            let key = Self::history_key("creator", creator.id(), &step, &before);
            if let Some(cached) = self.cache_get_json::<Vec<Vote>>(&key).await {
                result.insert(creator.id(), cached);
            } else {
                misses.push(creator);
            }
        }

        if !misses.is_empty() {
            let fetched = self
                .portfolio()
                .get_creator_history(&misses, &step, &before)
                .await?;
            for creator in &misses {
                let votes = fetched.get(&creator.id()).cloned().unwrap_or_default();
                let key = Self::history_key("creator", creator.id(), &step, &before);
                self.cache_set_json(&key, &votes, HISTORY_TTL).await;
                result.insert(creator.id(), votes);
            }
        }

        Ok(result)
    }

    pub async fn player_history(
        &self,
        players: &[Player],
        step: &TimeStep,
        before: &Duration,
    ) -> Result<HashMap<Id, Vec<PortfolioSnapshot>>> {
        let mut result = HashMap::new();
        let mut misses: Vec<&Player> = Vec::new();

        for player in players {
            let key = Self::history_key("player", player.id(), step, before);
            if let Some(cached) = self.cache_get_json::<Vec<PortfolioSnapshot>>(&key).await {
                result.insert(player.id(), cached);
            } else {
                misses.push(player);
            }
        }

        if !misses.is_empty() {
            let fetched = self
                .portfolio()
                .get_player_history(&misses, step, before)
                .await?;
            for player in &misses {
                let snapshots = fetched.get(&player.id()).cloned().unwrap_or_default();
                let key = Self::history_key("player", player.id(), step, before);
                self.cache_set_json(&key, &snapshots, HISTORY_TTL).await;
                result.insert(player.id(), snapshots);
            }
        }

        Ok(result)
    }
}
