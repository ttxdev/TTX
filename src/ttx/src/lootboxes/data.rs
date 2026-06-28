use chrono::Utc;
use sqlx::Row;
use sqlx::postgres::PgRow;

use crate::data::{Db, col_id, meta_from};
use crate::error::Result;
use crate::lootboxes::model::LootBox;
use crate::primitives::Id;

pub(crate) fn map_lootbox(row: &PgRow) -> sqlx::Result<LootBox> {
    Ok(LootBox {
        meta: meta_from(row)?,
        player_id: col_id(row, "player_id")?,
        result_id: row.try_get::<Option<i32>, _>("result_id")?.map(|v| v as Id),
    })
}

impl Db {
    pub async fn player_lootboxes(
        &self,
        player_id: Id,
        unopened_only: bool,
    ) -> Result<Vec<LootBox>> {
        let mut sql = String::from(
            "SELECT id, player_id, result_id, created_at, updated_at FROM loot_boxes WHERE player_id = $1",
        );
        if unopened_only {
            sql.push_str(" AND result_id IS NULL");
        }
        let rows = sqlx::query(&sql)
            .bind(player_id as i32)
            .fetch_all(&self.pool)
            .await?;
        rows.iter()
            .map(map_lootbox)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    pub async fn lootbox_by_id(&self, id: Id) -> Result<Option<LootBox>> {
        let row = sqlx::query(
            "SELECT id, player_id, result_id, created_at, updated_at FROM loot_boxes WHERE id = $1",
        )
        .bind(id as i32)
        .fetch_optional(&self.pool)
        .await?;
        Ok(row.as_ref().map(map_lootbox).transpose()?)
    }

    pub async fn insert_lootbox(&self, lootbox: &mut LootBox) -> Result<()> {
        let id: i32 = sqlx::query_scalar(
            "INSERT INTO loot_boxes (player_id, result_id, created_at, updated_at) \
             VALUES ($1,$2,$3,$4) RETURNING id",
        )
        .bind(lootbox.player_id as i32)
        .bind(lootbox.result_id.map(|v| v as i32))
        .bind(lootbox.meta.created_at)
        .bind(lootbox.meta.updated_at)
        .fetch_one(&self.pool)
        .await?;
        lootbox.meta.id = id as Id;
        Ok(())
    }

    pub async fn update_lootbox_result(&self, lootbox: &LootBox) -> Result<()> {
        sqlx::query("UPDATE loot_boxes SET result_id=$1, updated_at=$2 WHERE id=$3")
            .bind(lootbox.result_id.map(|v| v as i32))
            .bind(Utc::now())
            .bind(lootbox.id() as i32)
            .execute(&self.pool)
            .await?;
        Ok(())
    }
}
