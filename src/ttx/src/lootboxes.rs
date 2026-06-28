//! LootBoxes feature module.

pub mod data;
pub mod dto;
pub mod events;
pub mod model;

pub use model::{CreatorRarity, LootBox, OpenLootBoxResult, Rarity};

use std::sync::{Arc, Mutex};

use rand::rngs::StdRng;
use rand::{RngExt, SeedableRng};
use serde::Deserialize;

use crate::data::Db;
use crate::error::{Error, Result};
use crate::events::EventDispatcher;
use crate::options::RandomOptions;
use crate::primitives::Id;

use dto::LootBoxResultDto;
use events::OpenLootBoxEvent;
// The lootbox flow also dispatches a transaction-created event; reuse the
// transaction feature's event rather than redefining it.
use crate::transactions::events::CreateTransactionEvent;

#[derive(Debug, Clone, Copy, Deserialize)]
pub struct OpenLootBoxRequest {
    pub player_id: Id,
    pub loot_box_id: Id,
}

fn rarity_weight(rarity: Rarity) -> i64 {
    match rarity {
        Rarity::Epic => 5,
        Rarity::Rare => 25,
        Rarity::Common => 50,
        Rarity::Pennies => 100,
    }
}

pub struct LootBoxService {
    db: Db,
    events: Arc<dyn EventDispatcher>,
    rng: Arc<Mutex<StdRng>>,
}

impl LootBoxService {
    pub fn new(db: Db, events: Arc<dyn EventDispatcher>, options: RandomOptions) -> Self {
        let rng = match options.seed {
            Some(seed) => StdRng::seed_from_u64(seed),
            None => StdRng::from_rng(&mut rand::rng()),
        };
        Self {
            db,
            events,
            rng: Arc::new(Mutex::new(rng)),
        }
    }

    pub async fn get_creator_rarities(&self) -> Result<Vec<CreatorRarity>> {
        let average = self.db.average_creator_value().await?;
        let creators = self.db.live_creators_with_min_value(average).await?;
        let sum: f64 = creators.iter().map(|c| c.value).sum();

        Ok(creators
            .into_iter()
            .map(|creator| CreatorRarity::create(sum, creator))
            .collect())
    }

    pub async fn open_loot_box(&self, player_id: Id, box_id: Id) -> Result<LootBoxResultDto> {
        let Some(mut player) = self.db.player_by_id(player_id).await? else {
            return Err(Error::not_found("Player"));
        };

        let rarities = self.get_creator_rarities().await?;

        let Some(mut lootbox) = self.db.lootbox_by_id(box_id).await? else {
            return Err(Error::not_found("LootBox"));
        };
        if lootbox.player_id != player.id() {
            return Err(Error::not_found("LootBox"));
        }
        if lootbox.is_open() {
            return Err(Error::InvalidAction(
                "Lootbox is already opened.".to_string(),
            ));
        }

        let total_weight: i64 = rarities.iter().map(|r| rarity_weight(r.rarity)).sum();
        if total_weight == 0 {
            // The C# original dereferenced a null selection here; surface a
            // clean error instead.
            return Err(Error::not_found("Creator"));
        }

        let mut roll = {
            let mut rng = self.rng.lock().expect("rng poisoned");
            rng.random_range(0..total_weight)
        };

        let mut selected: Option<CreatorRarity> = None;
        for rarity in &rarities {
            let weight = rarity_weight(rarity.rarity);
            if roll < weight {
                selected = Some(rarity.clone());
                break;
            }
            roll -= weight;
        }
        let selected = selected.expect("a rarity is selected when total_weight > 0");

        lootbox.set_result(&selected.creator)?;
        self.db.update_lootbox_result(&lootbox).await?;

        let mut tx = player.give(&selected.creator);
        self.db.insert_transaction(&mut tx).await?;

        let result = OpenLootBoxResult {
            lootbox,
            result: selected,
            rarities,
        };
        let dto = LootBoxResultDto::create(&result, &player, tx.id());

        self.events
            .dispatch(&CreateTransactionEvent::create(&tx, &player))
            .await?;
        self.events
            .dispatch(&OpenLootBoxEvent::create(&result, &player, tx.id()))
            .await?;

        Ok(dto)
    }
}
