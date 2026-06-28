use chrono::{DateTime, Utc};
use serde::Serialize;

use crate::creators::Creator;
use crate::creators::dto::CreatorPartialDto;
use crate::players::Player;
use crate::players::dto::PlayerPartialDto;
use crate::primitives::Id;
use crate::transactions::model::{Transaction, TransactionAction};

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct TransactionDto {
    pub id: Id,
    pub created_at: DateTime<Utc>,
    pub updated_at: DateTime<Utc>,
    pub quantity: u64,
    pub value: f64,
    pub action: TransactionAction,
    pub creator_id: Id,
    pub player_id: Id,
}

impl TransactionDto {
    fn create(tx: &Transaction) -> Self {
        Self {
            id: tx.id(),
            created_at: tx.meta.created_at,
            updated_at: tx.meta.updated_at,
            quantity: tx.quantity,
            value: tx.value,
            action: tx.action,
            creator_id: tx.creator_id,
            player_id: tx.player_id,
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct CreatorTransactionDto {
    #[serde(flatten)]
    pub base: TransactionDto,
    pub player: PlayerPartialDto,
}

impl CreatorTransactionDto {
    pub fn create(tx: &Transaction, player: &Player) -> Self {
        Self {
            base: TransactionDto::create(tx),
            player: PlayerPartialDto::create(player),
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct PlayerTransactionDto {
    #[serde(flatten)]
    pub base: TransactionDto,
    pub creator: CreatorPartialDto,
}

impl PlayerTransactionDto {
    pub fn create(tx: &Transaction, creator: &Creator) -> Self {
        Self {
            base: TransactionDto::create(tx),
            creator: CreatorPartialDto::create(creator),
        }
    }
}
