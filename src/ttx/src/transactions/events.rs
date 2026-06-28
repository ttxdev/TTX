use serde::Serialize;

use crate::events::impl_event;
use crate::players::Player;
use crate::transactions::dto::CreatorTransactionDto;
use crate::transactions::model::Transaction;

#[derive(Debug, Serialize)]
pub struct CreateTransactionEvent {
    pub transaction: CreatorTransactionDto,
}

impl CreateTransactionEvent {
    pub fn create(tx: &Transaction, player: &Player) -> Self {
        Self {
            transaction: CreatorTransactionDto::create(tx, player),
        }
    }
}
impl_event!(CreateTransactionEvent, "CreateTransactionEvent");
