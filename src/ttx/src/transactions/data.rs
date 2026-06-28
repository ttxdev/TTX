use sqlx::Row;
use sqlx::postgres::PgRow;

use crate::data::{Db, col_id, meta_from};
use crate::error::Result;
use crate::primitives::Id;
use crate::transactions::model::{Transaction, TransactionAction};

pub(crate) const TX_COLS: &str =
    "id, quantity, value, action, creator_id, player_id, created_at, updated_at";

fn action_to_str(a: TransactionAction) -> &'static str {
    match a {
        TransactionAction::Buy => "Buy",
        TransactionAction::Sell => "Sell",
        TransactionAction::Open => "Open",
    }
}

fn parse_action(s: &str) -> TransactionAction {
    match s {
        "Sell" => TransactionAction::Sell,
        "Open" => TransactionAction::Open,
        _ => TransactionAction::Buy,
    }
}

pub(crate) fn map_transaction(row: &PgRow) -> sqlx::Result<Transaction> {
    Ok(Transaction {
        meta: meta_from(row)?,
        quantity: row.try_get::<i32, _>("quantity")? as u64,
        value: row.try_get("value")?,
        action: parse_action(row.try_get::<String, _>("action")?.as_str()),
        creator_id: col_id(row, "creator_id")?,
        player_id: col_id(row, "player_id")?,
    })
}

impl Db {
    pub async fn creator_transactions(&self, creator_id: Id) -> Result<Vec<Transaction>> {
        let sql =
            format!("SELECT {TX_COLS} FROM transactions WHERE creator_id = $1 ORDER BY created_at");
        let rows = sqlx::query(&sql)
            .bind(creator_id as i32)
            .fetch_all(&self.pool)
            .await?;
        rows.iter()
            .map(map_transaction)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    pub async fn player_transactions(&self, player_id: Id) -> Result<Vec<Transaction>> {
        let sql =
            format!("SELECT {TX_COLS} FROM transactions WHERE player_id = $1 ORDER BY created_at");
        let rows = sqlx::query(&sql)
            .bind(player_id as i32)
            .fetch_all(&self.pool)
            .await?;
        rows.iter()
            .map(map_transaction)
            .collect::<sqlx::Result<_>>()
            .map_err(Into::into)
    }

    pub async fn insert_transaction(&self, tx: &mut Transaction) -> Result<()> {
        let sql = "INSERT INTO transactions \
            (quantity, value, action, creator_id, player_id, created_at, updated_at) \
            VALUES ($1,$2,$3,$4,$5,$6,$7) RETURNING id";
        let id: i32 = sqlx::query_scalar(sql)
            .bind(tx.quantity as i32)
            .bind(tx.value)
            .bind(action_to_str(tx.action))
            .bind(tx.creator_id as i32)
            .bind(tx.player_id as i32)
            .bind(tx.meta.created_at)
            .bind(tx.meta.updated_at)
            .fetch_one(&self.pool)
            .await?;
        tx.meta.id = id as Id;
        Ok(())
    }
}
