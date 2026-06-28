use chrono::{DateTime, Utc};
use serde::Serialize;

use super::model::CreatorOptOut;
use crate::platforms::{Platform, PlatformId};
use crate::primitives::Id;

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct CreatorOptOutDto {
    pub id: Id,
    pub created_at: DateTime<Utc>,
    pub updated_at: DateTime<Utc>,
    pub platform_id: PlatformId,
    pub platform: Platform,
}

impl CreatorOptOutDto {
    pub fn create(opt: &CreatorOptOut) -> Self {
        Self {
            id: opt.id(),
            created_at: opt.meta.created_at,
            updated_at: opt.meta.updated_at,
            platform_id: opt.platform_id.to_string(),
            platform: opt.platform,
        }
    }
}
