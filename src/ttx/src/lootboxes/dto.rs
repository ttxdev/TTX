use chrono::{DateTime, Utc};
use serde::Serialize;

use super::model::{CreatorRarity, OpenLootBoxResult, Rarity};
use crate::creators::Creator;
use crate::creators::dto::CreatorPartialDto;
use crate::lootboxes::model::LootBox;
use crate::players::Player;
use crate::players::dto::PlayerPartialDto;
use crate::primitives::Id;

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct LootBoxDto {
    pub id: Id,
    pub created_at: DateTime<Utc>,
    pub updated_at: DateTime<Utc>,
    pub is_open: bool,
    pub result: Option<CreatorPartialDto>,
    pub player: PlayerPartialDto,
}

impl LootBoxDto {
    pub fn create(lootbox: &LootBox, player: &Player, result: Option<&Creator>) -> Self {
        Self {
            id: lootbox.id(),
            created_at: lootbox.meta.created_at,
            updated_at: lootbox.meta.updated_at,
            is_open: lootbox.is_open(),
            result: result.map(CreatorPartialDto::create),
            player: PlayerPartialDto::create(player),
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct CreatorRarityDto {
    pub creator: CreatorPartialDto,
    pub rarity: Rarity,
}

impl CreatorRarityDto {
    pub fn create(rarity: &CreatorRarity) -> Self {
        Self {
            creator: CreatorPartialDto::create(&rarity.creator),
            rarity: rarity.rarity,
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct LootBoxResultDto {
    pub lootbox_id: Id,
    pub player: PlayerPartialDto,
    pub result: CreatorRarityDto,
    pub transaction_id: Id,
    pub rarities: Vec<CreatorRarityDto>,
}

impl LootBoxResultDto {
    pub fn create(result: &OpenLootBoxResult, player: &Player, transaction_id: Id) -> Self {
        Self {
            lootbox_id: result.lootbox.id(),
            player: PlayerPartialDto::create(player),
            result: CreatorRarityDto::create(&result.result),
            transaction_id,
            rarities: result
                .rarities
                .iter()
                .map(CreatorRarityDto::create)
                .collect(),
        }
    }
}
