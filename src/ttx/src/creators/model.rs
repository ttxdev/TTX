use std::collections::HashMap;

use chrono::Utc;

use crate::platforms::{Platform, PlatformUser};
use crate::primitives::{Credits, Id, Meta, Timestamp};
use crate::shares::Share;
use crate::transactions::Transaction;
use crate::users::User;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct StreamStatus {
    pub is_live: bool,
    pub started_at: Timestamp,
    pub ended_at: Timestamp,
}

impl StreamStatus {
    pub fn started(&mut self, at: Timestamp) {
        self.started_at = at;
        self.is_live = true;
    }

    pub fn ended(&mut self, at: Timestamp) {
        self.ended_at = at;
        self.is_live = false;
    }
}

impl Default for StreamStatus {
    fn default() -> Self {
        let now = Utc::now();
        Self {
            is_live: false,
            started_at: now,
            ended_at: now,
        }
    }
}

#[derive(Debug, Clone, PartialEq, serde::Serialize, serde::Deserialize)]
pub struct Vote {
    pub creator_id: Id,
    pub value: Credits,
    pub time: Timestamp,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Creator {
    pub user: User,
    pub ticker: String,
    pub value: Credits,
    pub stream_status: StreamStatus,
    pub transactions: Vec<Transaction>,
    pub history: Vec<Vote>,
}

impl Creator {
    pub const MIN_VALUE: Credits = 1.0;
    pub const STARTER_VALUE: Credits = 1.0;

    pub fn id(&self) -> Id {
        self.user.meta.id
    }

    pub fn get_shares(&self) -> Vec<Share> {
        let mut shares: HashMap<Id, Share> = HashMap::new();

        for tx in &self.transactions {
            let share = shares
                .entry(tx.player_id)
                .or_insert_with(|| Share::new(self.id(), tx.player_id));
            share.count(tx);
        }

        shares
            .into_values()
            .filter(|share| share.quantity > 0)
            .collect()
    }

    pub fn apply_net_change(&mut self, net_change: Credits) -> Vote {
        self.value = Self::MIN_VALUE.max(self.value + net_change);

        Vote {
            creator_id: self.id(),
            value: self.value,
            time: Utc::now(),
        }
    }

    pub fn create(user: &PlatformUser, ticker: String, platform: Platform) -> Self {
        Self {
            user: User {
                meta: Meta::default(),
                name: user.display_name.clone(),
                slug: user.username.clone(),
                platform_id: user.id.clone(),
                platform,
                avatar_url: user.avatar_url.clone(),
            },
            ticker,
            value: Self::STARTER_VALUE,
            stream_status: StreamStatus::default(),
            transactions: Vec::new(),
            history: Vec::new(),
        }
    }
}
