use crate::creators::Creator;
use crate::error::{Error, Result};
use crate::players::Player;
use crate::primitives::{Credits, Id, Meta};

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct LootBox {
    pub meta: Meta,
    pub player_id: Id,
    pub result_id: Option<Id>,
}

#[derive(
    Debug, Clone, Copy, PartialEq, Eq, Hash, serde::Serialize, serde::Deserialize, utoipa::ToSchema,
)]
pub enum Rarity {
    Pennies,
    Common,
    Rare,
    Epic,
}

/// The outcome of opening a lootbox: the won creator plus the full set of
/// candidate rarities it was drawn from.
#[derive(Debug, Clone, PartialEq)]
pub struct OpenLootBoxResult {
    pub lootbox: LootBox,
    pub result: CreatorRarity,
    pub rarities: Vec<CreatorRarity>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct CreatorRarity {
    pub creator: Creator,
    pub rarity: Rarity,
}

impl CreatorRarity {
    pub fn create(sum: Credits, creator: Creator) -> Self {
        let calc = creator.value / sum * 100.0;
        let rarity = if (0.0..1.0).contains(&calc) {
            Rarity::Pennies
        } else if (1.0..5.0).contains(&calc) {
            Rarity::Common
        } else if (5.0..20.0).contains(&calc) {
            Rarity::Rare
        } else {
            Rarity::Epic
        };

        Self { creator, rarity }
    }
}

impl LootBox {
    pub fn id(&self) -> Id {
        self.meta.id
    }

    pub fn is_open(&self) -> bool {
        self.result_id.is_some()
    }

    pub fn create(player: &Player) -> Self {
        Self {
            meta: Meta::default(),
            player_id: player.id(),
            result_id: None,
        }
    }

    pub fn set_result(&mut self, creator: &Creator) -> Result<()> {
        if self.is_open() {
            return Err(Error::InvalidAction(
                "Lootbox is already opened.".to_string(),
            ));
        }

        self.result_id = Some(creator.id());

        Ok(())
    }
}
