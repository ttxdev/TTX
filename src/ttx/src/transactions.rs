pub mod data;
pub mod dto;
pub mod events;
pub mod model;

pub use model::{Transaction, TransactionAction};

use std::sync::Arc;

use crate::data::Db;
use crate::error::{Error, Result};
use crate::events::EventDispatcher;
use crate::primitives::{Id, Quantity};

use events::CreateTransactionEvent;

pub struct TransactionService {
    db: Db,
    events: Arc<dyn EventDispatcher>,
}

impl TransactionService {
    pub fn new(db: Db, events: Arc<dyn EventDispatcher>) -> Self {
        Self { db, events }
    }

    pub async fn place_order(
        &self,
        actor_id: Id,
        creator_slug: &str,
        action: TransactionAction,
        quantity: Quantity,
    ) -> Result<Id> {
        let Some(mut player) = self.db.player_by_id(actor_id).await? else {
            return Err(Error::not_found("Player"));
        };
        player.transactions = self.db.player_transactions(player.id()).await?;

        let Some(creator) = self.db.creator_by_slug(creator_slug).await? else {
            return Err(Error::not_found("Creator"));
        };

        if !creator.stream_status.is_live {
            return Err(Error::InvalidAction("Market is closed".to_string()));
        }

        let mut tx = match action {
            TransactionAction::Buy => player.buy(&creator, quantity)?,
            TransactionAction::Sell => player.sell(&creator, quantity)?,
            TransactionAction::Open => {
                return Err(Error::InvalidAction(
                    "Invalid transaction action".to_string(),
                ));
            }
        };

        self.db.update_player_balances(&player).await?;
        self.db.insert_transaction(&mut tx).await?;

        self.events
            .dispatch(&CreateTransactionEvent::create(&tx, &player))
            .await?;

        Ok(tx.id())
    }
}
