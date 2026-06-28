use chrono::Utc;
use std::collections::HashMap;

use crate::creators::Creator;
use crate::error::{Error, Result};
use crate::lootboxes::LootBox;
use crate::platforms::{Platform, PlatformUser};
use crate::primitives::{Credits, Id, Meta, Quantity, Timestamp};
use crate::shares::Share;
use crate::transactions::Transaction;
use crate::users::User;

pub const MAX_SHARES: Quantity = 1_000;
pub const MIN_CREDITS: Credits = 0.0;
pub const MIN_PORTFOLIO: Credits = 0.0;
pub const STARTER_CREDITS: Credits = 100.0;

#[derive(
    Debug,
    Clone,
    Copy,
    PartialEq,
    Eq,
    Hash,
    Default,
    serde::Serialize,
    serde::Deserialize,
    utoipa::ToSchema,
)]
pub enum PlayerType {
    #[default]
    User,
    Admin,
}

#[derive(Debug, Clone, PartialEq, serde::Serialize, serde::Deserialize)]
pub struct PortfolioSnapshot {
    pub player_id: Id,
    pub value: Credits,
    pub time: Timestamp,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Player {
    pub user: User,
    pub credits: Credits,
    pub portfolio: Credits,
    pub kind: PlayerType,
    pub history: Vec<PortfolioSnapshot>,
    pub transactions: Vec<Transaction>,
    pub lootboxes: Vec<LootBox>,
}

impl Player {
    pub fn id(&self) -> Id {
        self.user.meta.id
    }

    pub fn value(&self) -> Credits {
        self.credits + self.portfolio
    }

    pub fn get_shares(&self) -> Vec<Share> {
        let mut shares: HashMap<Id, Share> = HashMap::new();

        for tx in &self.transactions {
            let share = shares
                .entry(tx.creator_id)
                .or_insert_with(|| Share::new(tx.creator_id, self.id()));
            share.count(tx);
        }

        shares
            .into_values()
            .filter(|share| share.quantity > 0)
            .collect()
    }

    pub fn give(&mut self, creator: &Creator) -> Transaction {
        let tx = Transaction::create_open(self, creator, 1);
        self.transactions.push(tx.clone());

        tx
    }

    pub fn buy(&mut self, creator: &Creator, amount: Quantity) -> Result<Transaction> {
        let value = creator.value * amount as f64;
        if self.credits < value {
            return Err(Error::InvalidAction("Insufficient funds.".to_string()));
        }

        let current_quantity = self
            .get_shares()
            .into_iter()
            .find(|share| share.creator_id == creator.id())
            .map_or(0, |share| share.quantity);

        if current_quantity + amount > MAX_SHARES {
            return Err(Error::InvalidAction(format!(
                "Met max shares ({MAX_SHARES})."
            )));
        }

        self.credits -= value;
        let tx = Transaction::create_buy(self, creator, amount);
        self.transactions.push(tx.clone());

        Ok(tx)
    }

    pub fn sell(&mut self, creator: &Creator, amount: Quantity) -> Result<Transaction> {
        let quantity = self
            .get_shares()
            .into_iter()
            .find(|share| share.creator_id == creator.id())
            .map_or(0, |share| share.quantity);

        if quantity < amount {
            return Err(Error::InvalidAction("Insufficient shares.".to_string()));
        }

        let value = creator.value * amount as f64;
        self.credits += value;

        let tx = Transaction::create_sell(self, creator, amount);
        self.transactions.push(tx.clone());

        Ok(tx)
    }

    pub fn take_portfolio_snapshot(
        &mut self,
        creator_value: impl Fn(Id) -> Credits,
    ) -> PortfolioSnapshot {
        self.portfolio = self.get_shares().into_iter().fold(0.0, |acc, share| {
            acc + creator_value(share.creator_id) * share.quantity as f64
        });

        PortfolioSnapshot {
            player_id: self.id(),
            value: self.portfolio,
            time: Utc::now(),
        }
    }

    pub fn add_lootbox(&mut self) -> LootBox {
        let lootbox = LootBox::create(self);
        self.lootboxes.push(lootbox);

        lootbox
    }

    pub fn create(user: &PlatformUser, credits: Option<Credits>) -> Self {
        let mut player = Self {
            user: User {
                meta: Meta::default(),
                name: user.display_name.clone(),
                slug: user.username.clone(),
                platform_id: user.id.clone(),
                platform: Platform::Twitch,
                avatar_url: user.avatar_url.clone(),
            },
            credits: credits.unwrap_or(STARTER_CREDITS),
            portfolio: MIN_PORTFOLIO,
            kind: PlayerType::User,
            history: Vec::new(),
            transactions: Vec::new(),
            lootboxes: Vec::new(),
        };

        player.add_lootbox();

        player
    }
}
