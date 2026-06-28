use std::collections::HashMap;

use chrono::{DateTime, Duration, Utc};
use sqlx::{PgPool, Row};

use crate::creators::{Creator, Vote};
use crate::dto::portfolio::TimeStep;
use crate::error::Result;
use crate::players::{Player, PortfolioSnapshot};
use crate::primitives::Id;

pub struct PortfolioRepository {
    pool: PgPool,
}

impl PortfolioRepository {
    pub fn new(pool: PgPool) -> Self {
        Self { pool }
    }

    pub async fn store_vote(&self, vote: &Vote) -> Result<()> {
        sqlx::query("INSERT INTO votes (creator_id, value, time) VALUES ($1, $2, $3)")
            .bind(vote.creator_id as i32)
            .bind(vote.value)
            .bind(vote.time)
            .execute(&self.pool)
            .await?;
        Ok(())
    }

    pub async fn store_snapshot(&self, snapshot: &PortfolioSnapshot) -> Result<()> {
        sqlx::query("INSERT INTO player_portfolios (player_id, value, time) VALUES ($1, $2, $3)")
            .bind(snapshot.player_id as i32)
            .bind(snapshot.value)
            .bind(snapshot.time)
            .execute(&self.pool)
            .await?;
        Ok(())
    }

    pub async fn get_player_history(
        &self,
        players: &[&Player],
        step: &TimeStep,
        before: &Duration,
    ) -> Result<HashMap<Id, Vec<PortfolioSnapshot>>> {
        if players.is_empty() {
            return Ok(HashMap::new());
        }

        let interval = step.to_timescale_string();
        let player_ids: Vec<i32> = players.iter().map(|p| p.id() as i32).collect();
        let end_time = Utc::now();
        let start_time = end_time - *before;

        let sql = r#"
            SELECT
                p.player_id AS "player_id",
                time_bucket_gapfill($2::interval, p.time, $3, $4) AS "bucket",
                locf(
                    last(p.value, p.time),
                    (SELECT p2.value FROM player_portfolios p2
                     WHERE p2.player_id = p.player_id AND p2.time < $3
                     ORDER BY p2.time DESC LIMIT 1)
                ) AS "value"
            FROM player_portfolios p
            WHERE p.player_id = ANY($1)
                AND p.time >= $3
                AND p.time <= $4
            GROUP BY "player_id", "bucket"
            ORDER BY "bucket" ASC"#;

        let rows = sqlx::query(sql)
            .bind(&player_ids)
            .bind(interval)
            .bind(start_time)
            .bind(end_time)
            .fetch_all(&self.pool)
            .await?;

        let mut result: HashMap<Id, Vec<PortfolioSnapshot>> = HashMap::new();
        for row in &rows {
            let value: Option<f64> = row.try_get("value")?;
            let Some(value) = value else { continue };
            let player_id = row.try_get::<i32, _>("player_id")? as Id;
            let time: DateTime<Utc> = row.try_get("bucket")?;
            result
                .entry(player_id)
                .or_default()
                .push(PortfolioSnapshot {
                    player_id,
                    value,
                    time,
                });
        }

        Ok(result)
    }

    pub async fn get_creator_history(
        &self,
        creators: &[&Creator],
        step: &TimeStep,
        before: &Duration,
    ) -> Result<HashMap<Id, Vec<Vote>>> {
        if creators.is_empty() {
            return Ok(HashMap::new());
        }

        let interval = step.to_timescale_string();
        let now = Utc::now();

        // `time_bucket_gapfill` requires a constant time range per query, and
        // each creator's window differs (live → now, else its `ended_at`), so
        // these can't merge into one query. Instead run them concurrently; the
        // connection pool bounds the actual parallelism.
        let sql = r#"
            SELECT
                time_bucket_gapfill($2::interval, v.time, $3, $4) AS "bucket",
                locf(
                    last(v.value, v.time),
                    (SELECT v2.value FROM votes v2
                     WHERE v2.creator_id = $1 AND v2.time < $3
                     ORDER BY v2.time DESC LIMIT 1)
                ) AS "value"
            FROM votes v
            WHERE v.creator_id = $1
                AND v.time >= $3
                AND v.time <= $4
            GROUP BY "bucket"
            ORDER BY "bucket" ASC"#;

        let queries = creators.iter().map(|creator| {
            let id = creator.id();
            let end_time = if creator.stream_status.is_live {
                now
            } else {
                creator.stream_status.ended_at
            };
            let start_time = end_time - *before;
            let pool = &self.pool;

            async move {
                let rows = sqlx::query(sql)
                    .bind(id as i32)
                    .bind(interval)
                    .bind(start_time)
                    .bind(end_time)
                    .fetch_all(pool)
                    .await?;

                let mut votes = Vec::with_capacity(rows.len());
                for row in &rows {
                    let value: Option<f64> = row.try_get("value")?;
                    let Some(value) = value else { continue };
                    let time: DateTime<Utc> = row.try_get("bucket")?;
                    votes.push(Vote {
                        creator_id: id,
                        value,
                        time,
                    });
                }
                Ok::<(Id, Vec<Vote>), sqlx::Error>((id, votes))
            }
        });

        let result = futures_util::future::try_join_all(queries)
            .await?
            .into_iter()
            .collect();

        Ok(result)
    }
}
