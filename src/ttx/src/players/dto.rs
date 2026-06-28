use std::collections::HashMap;

use chrono::{DateTime, Utc};
use serde::Serialize;

use crate::creators::Creator;
use crate::creators::dto::CreatorPartialDto;
use crate::lootboxes::dto::LootBoxDto;
use crate::platforms::Platform;
use crate::players::model::{Player, PlayerType, PortfolioSnapshot};
use crate::primitives::Id;
use crate::shares::Share;
use crate::transactions::dto::PlayerTransactionDto;

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct PortfolioSnapshotDto {
    pub player_id: Id,
    pub value: f64,
    pub time: DateTime<Utc>,
}

impl PortfolioSnapshotDto {
    pub fn create(snapshot: &PortfolioSnapshot) -> Self {
        Self {
            player_id: snapshot.player_id,
            value: snapshot.value,
            time: snapshot.time,
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct PlayerPartialDto {
    pub id: Id,
    pub created_at: DateTime<Utc>,
    pub updated_at: DateTime<Utc>,
    pub name: String,
    pub slug: String,
    pub platform_id: String,
    pub platform: Platform,
    pub platform_url: String,
    pub avatar_url: String,
    pub credits: f64,
    pub portfolio: f64,
    pub value: f64,
    #[serde(rename = "type")]
    pub kind: PlayerType,
}

impl PlayerPartialDto {
    pub fn create(player: &Player) -> Self {
        Self {
            id: player.id(),
            created_at: player.user.meta.created_at,
            updated_at: player.user.meta.updated_at,
            name: player.user.name.clone(),
            slug: player.user.slug.clone(),
            platform_id: player.user.platform_id.to_string(),
            platform: player.user.platform,
            platform_url: player.user.platform.url(&player.user.slug),
            avatar_url: player.user.avatar_url.clone(),
            credits: player.credits,
            portfolio: player.portfolio,
            value: player.value(),
            kind: player.kind,
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct PlayerShareDto {
    pub creator: CreatorPartialDto,
    pub quantity: u64,
}

impl PlayerShareDto {
    pub fn create(share: &Share, creator: &Creator) -> Self {
        Self {
            creator: CreatorPartialDto::create(creator),
            quantity: share.quantity,
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct PlayerDto {
    #[serde(flatten)]
    pub partial: PlayerPartialDto,
    pub transactions: Vec<PlayerTransactionDto>,
    pub loot_boxes: Vec<LootBoxDto>,
    pub shares: Vec<PlayerShareDto>,
    pub history: Vec<PortfolioSnapshotDto>,
}

impl PlayerDto {
    /// `creators` resolves the creator referenced by each transaction, share,
    /// and (opened) lootbox result.
    pub fn create(player: &Player, creators: &HashMap<Id, Creator>) -> Self {
        let transactions = player
            .transactions
            .iter()
            .filter_map(|tx| {
                creators
                    .get(&tx.creator_id)
                    .map(|creator| PlayerTransactionDto::create(tx, creator))
            })
            .collect();

        let loot_boxes = player
            .lootboxes
            .iter()
            .map(|lootbox| {
                let result = lootbox.result_id.and_then(|id| creators.get(&id));
                LootBoxDto::create(lootbox, player, result)
            })
            .collect();

        let shares = player
            .get_shares()
            .iter()
            .filter_map(|share| {
                creators
                    .get(&share.creator_id)
                    .map(|creator| PlayerShareDto::create(share, creator))
            })
            .collect();

        let history = player
            .history
            .iter()
            .map(PortfolioSnapshotDto::create)
            .collect();

        Self {
            partial: PlayerPartialDto::create(player),
            transactions,
            loot_boxes,
            shares,
            history,
        }
    }
}
