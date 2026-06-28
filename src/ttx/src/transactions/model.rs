use crate::creators::Creator;
use crate::players::Player;
use crate::primitives::{Credits, Id, Meta, Quantity};

#[derive(
    Debug, Clone, Copy, PartialEq, Eq, Hash, serde::Serialize, serde::Deserialize, utoipa::ToSchema,
)]
pub enum TransactionAction {
    Buy,
    Sell,
    Open,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Transaction {
    pub meta: Meta,
    pub quantity: Quantity,
    pub value: Credits,
    pub action: TransactionAction,
    pub creator_id: Id,
    pub player_id: Id,
}

impl Transaction {
    pub fn id(&self) -> Id {
        self.meta.id
    }

    pub fn total(&self) -> Credits {
        self.value * self.quantity as f64
    }

    pub fn is_gain(&self) -> bool {
        matches!(
            self.action,
            TransactionAction::Buy | TransactionAction::Open
        )
    }

    pub fn create(
        actor: &Player,
        creator: &Creator,
        amount: Quantity,
        action: TransactionAction,
    ) -> Self {
        Self {
            meta: Meta::default(),
            player_id: actor.id(),
            creator_id: creator.id(),
            quantity: amount,
            action,
            value: creator.value,
        }
    }

    pub fn create_buy(actor: &Player, creator: &Creator, amount: Quantity) -> Self {
        Self::create(actor, creator, amount, TransactionAction::Buy)
    }

    pub fn create_sell(actor: &Player, creator: &Creator, amount: Quantity) -> Self {
        Self::create(actor, creator, amount, TransactionAction::Sell)
    }

    pub fn create_open(actor: &Player, creator: &Creator, amount: Quantity) -> Self {
        Self::create(actor, creator, amount, TransactionAction::Open)
    }
}
