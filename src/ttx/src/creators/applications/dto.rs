use chrono::{DateTime, Utc};
use serde::Serialize;

use super::model::{CreatorApplication, CreatorApplicationStatus};
use crate::platforms::Platform;
use crate::players::Player;
use crate::players::dto::PlayerPartialDto;
use crate::primitives::Id;

#[derive(Debug, PartialEq, Serialize, utoipa::ToSchema)]
pub struct CreatorApplicationDto {
    pub id: Id,
    pub created_at: DateTime<Utc>,
    pub updated_at: DateTime<Utc>,
    pub platform: Platform,
    pub platform_id: String,
    pub ticker: String,
    pub status: CreatorApplicationStatus,
    pub submitter: PlayerPartialDto,
}

impl CreatorApplicationDto {
    pub fn create(app: &CreatorApplication, submitter: &Player) -> Self {
        Self {
            id: app.id(),
            created_at: app.meta.created_at,
            updated_at: app.meta.updated_at,
            platform: app.platform,
            platform_id: app.platform_id.to_string(),
            ticker: app.ticker.clone(),
            status: app.status,
            submitter: PlayerPartialDto::create(submitter),
        }
    }
}
