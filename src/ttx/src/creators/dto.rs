use std::collections::HashMap;

use chrono::{DateTime, Utc};
use serde::Serialize;

use crate::creators::model::{Creator, StreamStatus, Vote};
use crate::players::Player;
use crate::players::dto::PlayerPartialDto;
use crate::primitives::Id;
use crate::shares::Share;
use crate::transactions::dto::CreatorTransactionDto;

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct StreamStatusDto {
    pub is_live: bool,
    pub started_at: Option<DateTime<Utc>>,
    pub ended_at: Option<DateTime<Utc>>,
}

impl StreamStatusDto {
    pub fn create(status: &StreamStatus) -> Self {
        Self {
            is_live: status.is_live,
            started_at: Some(status.started_at),
            ended_at: Some(status.ended_at),
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct VoteDto {
    pub creator_id: Id,
    pub value: f64,
    pub time: DateTime<Utc>,
}

impl VoteDto {
    pub fn create(vote: &Vote) -> Self {
        Self {
            creator_id: vote.creator_id,
            value: vote.value,
            time: vote.time,
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct CreatorPartialDto {
    pub id: Id,
    pub created_at: DateTime<Utc>,
    pub updated_at: DateTime<Utc>,
    pub name: String,
    pub slug: String,
    pub platform_id: String,
    pub platform: crate::platforms::Platform,
    pub platform_url: String,
    pub avatar_url: String,
    pub ticker: String,
    pub value: f64,
    pub stream_status: StreamStatusDto,
    pub history: Vec<VoteDto>,
}

impl CreatorPartialDto {
    pub fn create(creator: &Creator) -> Self {
        Self {
            id: creator.id(),
            created_at: creator.user.meta.created_at,
            updated_at: creator.user.meta.updated_at,
            name: creator.user.name.clone(),
            slug: creator.user.slug.clone(),
            platform_id: creator.user.platform_id.to_string(),
            platform: creator.user.platform,
            platform_url: creator.user.platform.url(&creator.user.slug),
            avatar_url: creator.user.avatar_url.clone(),
            ticker: creator.ticker.clone(),
            value: creator.value,
            stream_status: StreamStatusDto::create(&creator.stream_status),
            history: creator.history.iter().map(VoteDto::create).collect(),
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct CreatorShareDto {
    pub player: PlayerPartialDto,
    pub quantity: u64,
}

impl CreatorShareDto {
    pub fn create(share: &Share, player: &Player) -> Self {
        Self {
            player: PlayerPartialDto::create(player),
            quantity: share.quantity,
        }
    }
}

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct CreatorDto {
    #[serde(flatten)]
    pub partial: CreatorPartialDto,
    pub transactions: Vec<CreatorTransactionDto>,
    pub shares: Vec<CreatorShareDto>,
}

impl CreatorDto {
    /// `players` resolves the player referenced by each transaction/share.
    pub fn create(creator: &Creator, players: &HashMap<Id, Player>) -> Self {
        let transactions = creator
            .transactions
            .iter()
            .filter_map(|tx| {
                players
                    .get(&tx.player_id)
                    .map(|player| CreatorTransactionDto::create(tx, player))
            })
            .collect();

        let shares = creator
            .get_shares()
            .iter()
            .filter_map(|share| {
                players
                    .get(&share.player_id)
                    .map(|player| CreatorShareDto::create(share, player))
            })
            .collect();

        Self {
            partial: CreatorPartialDto::create(creator),
            transactions,
            shares,
        }
    }
}
