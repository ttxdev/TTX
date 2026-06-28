use std::collections::HashMap;

use serde::Serialize;

use crate::creators::Creator;
use crate::events::impl_event;
use crate::players::dto::{PlayerDto, PlayerPartialDto, PortfolioSnapshotDto};
use crate::players::model::{Player, PortfolioSnapshot};
use crate::primitives::Id;

#[derive(Debug, Serialize)]
pub struct CreatePlayerEvent {
    pub player: PlayerDto,
}

impl CreatePlayerEvent {
    pub fn create(player: &Player, creators: &HashMap<Id, Creator>) -> Self {
        Self {
            player: PlayerDto::create(player, creators),
        }
    }
}
impl_event!(CreatePlayerEvent, "CreatePlayerEvent");

#[derive(Debug, Serialize)]
pub struct UpdatePlayerPortfolioEvent {
    pub player: PlayerPartialDto,
    pub snapshot: PortfolioSnapshotDto,
}

impl UpdatePlayerPortfolioEvent {
    pub fn create(snapshot: &PortfolioSnapshot, player: &Player) -> Self {
        Self {
            player: PlayerPartialDto::create(player),
            snapshot: PortfolioSnapshotDto::create(snapshot),
        }
    }
}
impl_event!(UpdatePlayerPortfolioEvent, "UpdatePlayerPortfolioEvent");
