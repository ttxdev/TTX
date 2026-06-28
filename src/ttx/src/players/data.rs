//! Player persistence. Additional `impl Db` block over the shared pool.

use chrono::Utc;
use sqlx::Row;
use sqlx::postgres::PgRow;

use crate::data::{Db, meta_from, parse_platform, platform_to_str};
use crate::error::Result;
use crate::platforms::Platform;
use crate::players::model::{Player, PlayerType, PortfolioSnapshot};
use crate::primitives::Id;
use crate::users::User;

pub(crate) const PLAYER_COLS: &str = "id, name, slug, platform, platform_id, avatar_url, credits, portfolio, \
     type, created_at, updated_at";

fn player_type_to_str(t: PlayerType) -> &'static str {
    match t {
        PlayerType::User => "User",
        PlayerType::Admin => "Admin",
    }
}

fn parse_player_type(s: &str) -> PlayerType {
    match s {
        "Admin" => PlayerType::Admin,
        _ => PlayerType::User,
    }
}

pub(crate) fn map_player(row: &PgRow) -> sqlx::Result<Player> {
    let platform_id: String = row.try_get("platform_id")?;
    Ok(Player {
        user: User {
            meta: meta_from(row)?,
            name: row.try_get("name")?,
            slug: row.try_get("slug")?,
            platform_id,
            platform: parse_platform(row.try_get::<String, _>("platform")?.as_str()),
            avatar_url: row.try_get("avatar_url")?,
        },
        credits: row.try_get("credits")?,
        portfolio: row.try_get("portfolio")?,
        kind: parse_player_type(row.try_get::<String, _>("type")?.as_str()),
        history: Vec::new(),
        transactions: Vec::new(),
        lootboxes: Vec::new(),
    })
}

impl Db {
    pub async fn count_players(&self, search: Option<&str>) -> Result<i64> {
        let like = search.map(|s| format!("%{s}%"));
        let count: i64 = sqlx::query_scalar(
            "SELECT COUNT(*) FROM players WHERE ($1::text IS NULL OR name ILIKE $1)",
        )
        .bind(like)
        .fetch_one(&self.pool)
        .await?;
        Ok(count)
    }

    pub async fn index_players(
        &self,
        search: Option<&str>,
        order_clause: &str,
        offset: i64,
        limit: i64,
    ) -> Result<Vec<Player>> {
        let sql = format!(
            "SELECT {PLAYER_COLS} FROM players WHERE ($1::text IS NULL OR name ILIKE $1) \
             ORDER BY {order_clause} LIMIT $2 OFFSET $3"
        );
        let like = search.map(|s| format!("%{s}%"));
        let rows = sqlx::query(&sql)
            .bind(like)
            .bind(limit)
            .bind(offset)
            .fetch_all(&self.pool)
            .await?;
        rows.iter()
            .map(map_player)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    pub async fn player_by_id(&self, id: Id) -> Result<Option<Player>> {
        let sql = format!("SELECT {PLAYER_COLS} FROM players WHERE id = $1");
        let row = sqlx::query(&sql)
            .bind(id as i32)
            .fetch_optional(&self.pool)
            .await?;
        Ok(row.as_ref().map(map_player).transpose()?)
    }

    pub async fn player_by_slug(&self, slug: &str) -> Result<Option<Player>> {
        let sql = format!("SELECT {PLAYER_COLS} FROM players WHERE slug = $1");
        let row = sqlx::query(&sql)
            .bind(slug)
            .fetch_optional(&self.pool)
            .await?;
        Ok(row.as_ref().map(map_player).transpose()?)
    }

    pub async fn player_by_platform(
        &self,
        platform: Platform,
        platform_id: &str,
    ) -> Result<Option<Player>> {
        let sql =
            format!("SELECT {PLAYER_COLS} FROM players WHERE platform = $1 AND platform_id = $2");
        let row = sqlx::query(&sql)
            .bind(platform_to_str(platform))
            .bind(platform_id)
            .fetch_optional(&self.pool)
            .await?;
        Ok(row.as_ref().map(map_player).transpose()?)
    }

    pub async fn players_by_ids(&self, ids: &[Id]) -> Result<Vec<Player>> {
        if ids.is_empty() {
            return Ok(Vec::new());
        }
        let int_ids: Vec<i32> = ids.iter().map(|&i| i as i32).collect();
        let sql = format!("SELECT {PLAYER_COLS} FROM players WHERE id = ANY($1)");
        let rows = sqlx::query(&sql)
            .bind(&int_ids)
            .fetch_all(&self.pool)
            .await?;
        rows.iter()
            .map(map_player)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    pub async fn all_players(&self) -> Result<Vec<Player>> {
        let sql = format!("SELECT {PLAYER_COLS} FROM players");
        let rows = sqlx::query(&sql).fetch_all(&self.pool).await?;
        rows.iter()
            .map(map_player)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    /// Inserts a player and its starter lootbox, populating generated ids.
    pub async fn insert_player(&self, player: &mut Player) -> Result<()> {
        let sql = "INSERT INTO players \
            (name, slug, platform, platform_id, avatar_url, credits, portfolio, type, \
             created_at, updated_at) \
            VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10) RETURNING id";
        let id: i32 = sqlx::query_scalar(sql)
            .bind(&player.user.name)
            .bind(&player.user.slug)
            .bind(platform_to_str(player.user.platform))
            .bind(player.user.platform_id.to_string())
            .bind(&player.user.avatar_url)
            .bind(player.credits)
            .bind(player.portfolio)
            .bind(player_type_to_str(player.kind))
            .bind(player.user.meta.created_at)
            .bind(player.user.meta.updated_at)
            .fetch_one(&self.pool)
            .await?;
        player.user.meta.id = id as Id;

        let player_id = player.id();
        for lootbox in &mut player.lootboxes {
            lootbox.player_id = player_id;
            self.insert_lootbox(lootbox).await?;
        }
        Ok(())
    }

    pub async fn update_player_profile(&self, player: &Player) -> Result<()> {
        sqlx::query(
            "UPDATE players SET name=$1, slug=$2, avatar_url=$3, updated_at=$4 WHERE id=$5",
        )
        .bind(&player.user.name)
        .bind(&player.user.slug)
        .bind(&player.user.avatar_url)
        .bind(Utc::now())
        .bind(player.id() as i32)
        .execute(&self.pool)
        .await?;
        Ok(())
    }

    pub async fn update_player_balances(&self, player: &Player) -> Result<()> {
        sqlx::query("UPDATE players SET credits=$1, portfolio=$2, updated_at=$3 WHERE id=$4")
            .bind(player.credits)
            .bind(player.portfolio)
            .bind(Utc::now())
            .bind(player.id() as i32)
            .execute(&self.pool)
            .await?;
        Ok(())
    }

    pub async fn commit_portfolio_snapshots(
        &self,
        players: &[Player],
        snapshots: &[PortfolioSnapshot],
    ) -> Result<()> {
        let now = Utc::now();
        let mut tx = self.pool.begin().await?;

        for (player, snapshot) in players.iter().zip(snapshots) {
            sqlx::query(
                "INSERT INTO player_portfolios (player_id, value, time) VALUES ($1, $2, $3)",
            )
            .bind(snapshot.player_id as i32)
            .bind(snapshot.value)
            .bind(snapshot.time)
            .execute(&mut *tx)
            .await?;
            sqlx::query("UPDATE players SET credits=$1, portfolio=$2, updated_at=$3 WHERE id=$4")
                .bind(player.credits)
                .bind(player.portfolio)
                .bind(now)
                .bind(player.id() as i32)
                .execute(&mut *tx)
                .await?;
        }

        tx.commit().await?;
        Ok(())
    }
}
