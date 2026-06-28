use crate::error::{Error, Result};
use crate::platforms::{Platform, PlatformId};
use crate::players::Player;
use crate::primitives::{Id, Meta};

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
pub enum CreatorApplicationStatus {
    #[default]
    Pending,
    Approved,
    Rejected,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct CreatorApplication {
    pub meta: Meta,
    pub submitter_id: Id,
    pub platform: Platform,
    pub platform_id: PlatformId,
    pub ticker: String,
    pub name: String,
    pub status: CreatorApplicationStatus,
}

impl CreatorApplication {
    pub fn id(&self) -> Id {
        self.meta.id
    }

    pub fn create(
        name: String,
        ticker: String,
        platform: Platform,
        platform_id: PlatformId,
        submitter: &Player,
    ) -> Self {
        Self {
            meta: Meta::default(),
            name,
            platform,
            platform_id,
            ticker,
            status: CreatorApplicationStatus::Pending,
            submitter_id: submitter.id(),
        }
    }

    pub fn update_status(&mut self, status: CreatorApplicationStatus) -> Result<()> {
        if matches!(
            self.status,
            CreatorApplicationStatus::Approved | CreatorApplicationStatus::Rejected
        ) {
            return Err(Error::InvalidAction(
                "Creator Application was already reviewed.".to_string(),
            ));
        }

        self.status = status;

        Ok(())
    }
}
