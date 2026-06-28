use serde::Serialize;

use super::model::OpenLootBoxResult;
use crate::creators::Creator;
use crate::events::impl_event;
use crate::lootboxes::dto::{LootBoxDto, LootBoxResultDto};
use crate::lootboxes::model::LootBox;
use crate::players::Player;
use crate::primitives::Id;

#[derive(Debug, Serialize)]
pub struct OpenLootBoxEvent {
    pub result: LootBoxResultDto,
}

impl OpenLootBoxEvent {
    pub fn create(result: &OpenLootBoxResult, player: &Player, transaction_id: Id) -> Self {
        Self {
            result: LootBoxResultDto::create(result, player, transaction_id),
        }
    }
}
impl_event!(OpenLootBoxEvent, "OpenLootBoxEvent");

#[derive(Debug, Serialize)]
pub struct CreateLooboxEvent {
    pub loot_box: LootBoxDto,
}

impl CreateLooboxEvent {
    pub fn create(lootbox: &LootBox, player: &Player, result: Option<&Creator>) -> Self {
        Self {
            loot_box: LootBoxDto::create(lootbox, player, result),
        }
    }
}
impl_event!(CreateLooboxEvent, "CreateLooboxEvent");
