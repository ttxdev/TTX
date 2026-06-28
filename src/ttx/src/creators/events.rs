use serde::Serialize;

use crate::creators::dto::{CreatorPartialDto, StreamStatusDto, VoteDto};
use crate::creators::model::{Creator, Vote};
use crate::events::impl_event;
use crate::primitives::Id;

#[derive(Debug, Serialize)]
pub struct CreateCreatorEvent {
    pub creator: CreatorPartialDto,
}

impl CreateCreatorEvent {
    pub fn create(creator: &Creator) -> Self {
        Self {
            creator: CreatorPartialDto::create(creator),
        }
    }
}
impl_event!(CreateCreatorEvent, "CreateCreatorEvent");

#[derive(Debug, Serialize)]
pub struct UpdateCreatorValueEvent {
    pub vote: VoteDto,
}

impl UpdateCreatorValueEvent {
    pub fn create(vote: &Vote) -> Self {
        Self {
            vote: VoteDto::create(vote),
        }
    }
}
impl_event!(UpdateCreatorValueEvent, "UpdateCreatorValueEvent");

#[derive(Debug, Serialize)]
pub struct UpdateStreamStatusEvent {
    pub creator_id: Id,
    pub stream_status: StreamStatusDto,
}

impl UpdateStreamStatusEvent {
    pub fn create(creator: &Creator) -> Self {
        Self {
            creator_id: creator.id(),
            stream_status: StreamStatusDto::create(&creator.stream_status),
        }
    }
}
impl_event!(UpdateStreamStatusEvent, "UpdateStreamStatusEvent");
