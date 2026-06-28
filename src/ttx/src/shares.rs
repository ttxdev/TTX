use crate::primitives::{Id, Quantity};
use crate::transactions::Transaction;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct Share {
    pub creator_id: Id,
    pub player_id: Id,
    pub quantity: Quantity,
}

impl Share {
    pub fn new(creator_id: Id, player_id: Id) -> Self {
        Self {
            creator_id,
            player_id,
            quantity: 0,
        }
    }

    pub fn count(&mut self, tx: &Transaction) {
        if tx.is_gain() {
            self.quantity += tx.quantity;
        } else {
            self.quantity = self.quantity.saturating_sub(tx.quantity);
        }
    }
}
