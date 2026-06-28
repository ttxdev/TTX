//! Shared persistence infrastructure, ported from `TTX.App.Data`.
//!
//! Entity Framework's `ApplicationDbContext` is replaced by [`Db`], a thin
//! wrapper over a `sqlx` connection pool. The actual queries live with their
//! feature modules (e.g. [`crate::players::data`]) as additional `impl Db`
//! blocks; this module holds the struct plus the conversion/mapping helpers
//! they share. Change-tracking is gone, so callers persist mutations
//! explicitly, and relationships (stored as ids) are hydrated by separate
//! queries — replacing EF `Include`.

pub mod portfolio;

use std::collections::HashMap;
use std::sync::Arc;
use std::time::Duration as StdDuration;

use chrono::{DateTime, Duration, Utc};
use serde::Serialize;
use serde::de::DeserializeOwned;
use sqlx::postgres::PgRow;
use sqlx::{PgPool, Row};

use crate::cache::{Cache, InMemoryCache};
use crate::creators::{Creator, Vote};
use crate::dto::portfolio::TimeStep;
use crate::error::Result;
use crate::platforms::Platform;
use crate::players::{Player, PortfolioSnapshot};
use crate::primitives::{Id, Meta};

pub use portfolio::PortfolioRepository;

/// History series are stale at most one job cycle; this bounds it (see
/// `docs/caching.md`). Event-driven invalidation can tighten it later.
const HISTORY_TTL: StdDuration = StdDuration::from_secs(15);
/// `average_creator_value` moves slowly; a longer TTL is fine.
pub(crate) const AVERAGE_TTL: StdDuration = StdDuration::from_secs(30);

// ---- shared enum <-> text/int conversions (mirror the EF `HasConversion`s) ----

/// `players`/`creators` store `platform` as the enum's text name.
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

/// `creator_opt_outs`/`creator_applications` store `Platform` as the raw enum
/// integer (EF's default enum mapping), unlike the text column used elsewhere.
pub(crate) fn platform_to_int(p: Platform) -> i32 {
    match p {
        Platform::Twitch => 0,
    }
}

// ---- shared row mappers ----

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
    /// Builds a `Db` with the default in-process cache. Convenient for tests,
    /// the seeder, and single-instance dev where no shared cache is configured.
    pub fn new(pool: PgPool) -> Self {
        Self::with_cache(pool, Arc::new(InMemoryCache::new()))
    }

    /// Builds a `Db` with an explicit cache backend (e.g. `crate::cache::RedisCache`).
    pub fn with_cache(pool: PgPool, cache: Arc<dyn Cache>) -> Self {
        Self { pool, cache }
    }

    pub fn portfolio(&self) -> PortfolioRepository {
        PortfolioRepository::new(self.pool.clone())
    }

    /// Reads and deserializes a cached JSON value; `None` on miss or any error.
    pub(crate) async fn cache_get_json<T: DeserializeOwned>(&self, key: &str) -> Option<T> {
        let raw = self.cache.get(key).await?;
        serde_json::from_str(&raw).ok()
    }

    /// Serializes and stores a value under `key` with the given TTL (best-effort).
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

    /// Cache key for a history series. The `kind` prefix keeps creator and
    /// player ids in separate keyspaces (both start at 1) now that a single
    /// cache backs both series.
    fn history_key(kind: &str, id: Id, step: &TimeStep, before: &Duration) -> String {
        format!("hist:{kind}:{id}:{step:?}:{}", before.num_seconds())
    }

    /// Cached creator history. Resolves per creator: cache hits are served
    /// directly and only the misses are queried (in one concurrent batch).
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

    /// Cached player history. Misses are fetched in a single `ANY(...)` query.
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
