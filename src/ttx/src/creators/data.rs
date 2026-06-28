//! Creator persistence. Additional `impl Db` block over the shared pool.

use chrono::Utc;
use sqlx::Row;
use sqlx::postgres::PgRow;

use crate::creators::model::{Creator, StreamStatus};
use crate::data::{AVERAGE_TTL, Db, meta_from, parse_platform, platform_to_str};
use crate::error::Result;
use crate::primitives::Id;
use crate::users::User;

pub(crate) const CREATOR_COLS: &str = "id, name, slug, ticker, platform, platform_id, avatar_url, value, \
     stream_is_live, stream_started_at, stream_ended_at, created_at, updated_at";

pub(crate) fn map_creator(row: &PgRow) -> sqlx::Result<Creator> {
    Ok(Creator {
        user: User {
            meta: meta_from(row)?,
            name: row.try_get("name")?,
            slug: row.try_get("slug")?,
            platform_id: row.try_get("platform_id")?,
            platform: parse_platform(row.try_get::<String, _>("platform")?.as_str()),
            avatar_url: row.try_get("avatar_url")?,
        },
        ticker: row.try_get("ticker")?,
        value: row.try_get("value")?,
        stream_status: StreamStatus {
            is_live: row.try_get("stream_is_live")?,
            started_at: row.try_get("stream_started_at")?,
            ended_at: row.try_get("stream_ended_at")?,
        },
        transactions: Vec::new(),
        history: Vec::new(),
    })
}

impl Db {
    /// Counts creators matching the index filters.
    pub async fn count_creators(
        &self,
        min_value: Option<f64>,
        search: Option<&str>,
    ) -> Result<i64> {
        let sql = "SELECT COUNT(*) FROM creators WHERE ($1::float8 IS NULL OR value >= $1) \
             AND ($2::text IS NULL OR name ILIKE $2)";
        let like = search.map(|s| format!("%{s}%"));
        let count: i64 = sqlx::query_scalar(sql)
            .bind(min_value)
            .bind(like)
            .fetch_one(&self.pool)
            .await?;
        Ok(count)
    }

    /// Fetches a page of creators. `order_clause` is a trusted static `ORDER BY`
    /// body built from a fixed enum (never user input).
    pub async fn index_creators(
        &self,
        min_value: Option<f64>,
        search: Option<&str>,
        order_clause: &str,
        offset: i64,
        limit: i64,
    ) -> Result<Vec<Creator>> {
        let sql = format!(
            "SELECT {CREATOR_COLS} FROM creators \
             WHERE ($1::float8 IS NULL OR value >= $1) AND ($2::text IS NULL OR name ILIKE $2) \
             ORDER BY {order_clause} LIMIT $3 OFFSET $4"
        );
        let like = search.map(|s| format!("%{s}%"));
        let rows = sqlx::query(&sql)
            .bind(min_value)
            .bind(like)
            .bind(limit)
            .bind(offset)
            .fetch_all(&self.pool)
            .await?;
        rows.iter()
            .map(map_creator)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    pub async fn creator_by_slug(&self, slug: &str) -> Result<Option<Creator>> {
        let sql = format!("SELECT {CREATOR_COLS} FROM creators WHERE slug = $1");
        let row = sqlx::query(&sql)
            .bind(slug)
            .fetch_optional(&self.pool)
            .await?;
        Ok(row.as_ref().map(map_creator).transpose()?)
    }

    pub async fn creator_by_platform(
        &self,
        platform: crate::platforms::Platform,
        platform_id: &str,
    ) -> Result<Option<Creator>> {
        let sql =
            format!("SELECT {CREATOR_COLS} FROM creators WHERE platform = $1 AND platform_id = $2");
        let row = sqlx::query(&sql)
            .bind(platform_to_str(platform))
            .bind(platform_id)
            .fetch_optional(&self.pool)
            .await?;
        Ok(row.as_ref().map(map_creator).transpose()?)
    }

    pub async fn creators_by_ids(&self, ids: &[Id]) -> Result<Vec<Creator>> {
        if ids.is_empty() {
            return Ok(Vec::new());
        }
        let int_ids: Vec<i32> = ids.iter().map(|&i| i as i32).collect();
        let sql = format!("SELECT {CREATOR_COLS} FROM creators WHERE id = ANY($1)");
        let rows = sqlx::query(&sql)
            .bind(&int_ids)
            .fetch_all(&self.pool)
            .await?;
        rows.iter()
            .map(map_creator)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    pub async fn all_creators(&self) -> Result<Vec<Creator>> {
        let sql = format!("SELECT {CREATOR_COLS} FROM creators");
        let rows = sqlx::query(&sql).fetch_all(&self.pool).await?;
        rows.iter()
            .map(map_creator)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    pub async fn live_creators(&self) -> Result<Vec<Creator>> {
        let sql = format!("SELECT {CREATOR_COLS} FROM creators WHERE stream_is_live = true");
        let rows = sqlx::query(&sql).fetch_all(&self.pool).await?;
        rows.iter()
            .map(map_creator)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    pub async fn live_creators_with_min_value(&self, min_value: f64) -> Result<Vec<Creator>> {
        let sql = format!(
            "SELECT {CREATOR_COLS} FROM creators WHERE stream_is_live = true AND value >= $1"
        );
        let rows = sqlx::query(&sql)
            .bind(min_value)
            .fetch_all(&self.pool)
            .await?;
        rows.iter()
            .map(map_creator)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    pub async fn average_creator_value(&self) -> Result<f64> {
        const KEY: &str = "avg:creator_value";
        if let Some(cached) = self.cache_get_json::<f64>(KEY).await {
            return Ok(cached);
        }
        let avg: Option<f64> = sqlx::query_scalar("SELECT AVG(value) FROM creators")
            .fetch_one(&self.pool)
            .await?;
        let avg = avg.unwrap_or(0.0);
        self.cache_set_json(KEY, &avg, AVERAGE_TTL).await;
        Ok(avg)
    }

    pub async fn ticker_exists(&self, ticker: &str) -> Result<bool> {
        let exists: bool =
            sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM creators WHERE ticker = $1)")
                .bind(ticker)
                .fetch_one(&self.pool)
                .await?;
        Ok(exists)
    }

    pub async fn insert_creator(&self, creator: &mut Creator) -> Result<()> {
        let sql = "INSERT INTO creators \
            (name, slug, ticker, platform, platform_id, avatar_url, value, \
             stream_is_live, stream_started_at, stream_ended_at, created_at, updated_at) \
            VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12) RETURNING id";
        let id: i32 = sqlx::query_scalar(sql)
            .bind(&creator.user.name)
            .bind(&creator.user.slug)
            .bind(&creator.ticker)
            .bind(platform_to_str(creator.user.platform))
            .bind(creator.user.platform_id.to_string())
            .bind(&creator.user.avatar_url)
            .bind(creator.value)
            .bind(creator.stream_status.is_live)
            .bind(creator.stream_status.started_at)
            .bind(creator.stream_status.ended_at)
            .bind(creator.user.meta.created_at)
            .bind(creator.user.meta.updated_at)
            .fetch_one(&self.pool)
            .await?;
        creator.user.meta.id = id as Id;
        Ok(())
    }

    /// Persists profile fields changed by `User.sync`.
    pub async fn update_creator_profile(&self, creator: &Creator) -> Result<()> {
        sqlx::query(
            "UPDATE creators SET name=$1, slug=$2, avatar_url=$3, updated_at=$4 WHERE id=$5",
        )
        .bind(&creator.user.name)
        .bind(&creator.user.slug)
        .bind(&creator.user.avatar_url)
        .bind(Utc::now())
        .bind(creator.id() as i32)
        .execute(&self.pool)
        .await?;
        Ok(())
    }

    pub async fn delete_creator(&self, id: Id) -> Result<()> {
        sqlx::query("DELETE FROM creators WHERE id = $1")
            .bind(id as i32)
            .execute(&self.pool)
            .await?;
        Ok(())
    }

    /// Applies a creator's new value plus the bump from `ApplyNetChange`.
    pub async fn update_creator_value(&self, creator: &Creator) -> Result<()> {
        sqlx::query("UPDATE creators SET value=$1, updated_at=$2 WHERE id=$3")
            .bind(creator.value)
            .bind(Utc::now())
            .bind(creator.id() as i32)
            .execute(&self.pool)
            .await?;
        Ok(())
    }

    pub async fn reset_all_streams_offline(&self) -> Result<()> {
        sqlx::query("UPDATE creators SET stream_is_live=false, stream_ended_at=$1, updated_at=$1")
            .bind(Utc::now())
            .execute(&self.pool)
            .await?;
        Ok(())
    }

    pub async fn update_stream_status(&self, creator: &Creator) -> Result<()> {
        sqlx::query(
            "UPDATE creators SET stream_is_live=$1, stream_started_at=$2, stream_ended_at=$3, \
             updated_at=$4 WHERE id=$5",
        )
        .bind(creator.stream_status.is_live)
        .bind(creator.stream_status.started_at)
        .bind(creator.stream_status.ended_at)
        .bind(Utc::now())
        .bind(creator.id() as i32)
        .execute(&self.pool)
        .await?;
        Ok(())
    }
}
