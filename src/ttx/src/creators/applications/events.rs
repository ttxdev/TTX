use serde::Serialize;

use super::dto::CreatorApplicationDto;
use super::model::CreatorApplication;
use crate::events::impl_event;
use crate::players::Player;

#[derive(Debug, Serialize)]
pub struct CreateCreatorApplicationEvent {
    pub application: CreatorApplicationDto,
}

impl CreateCreatorApplicationEvent {
    pub fn create(app: &CreatorApplication, submitter: &Player) -> Self {
        Self {
            application: CreatorApplicationDto::create(app, submitter),
        }
    }
}
impl_event!(
    CreateCreatorApplicationEvent,
    "CreateCreatorApplicationEvent"
);

#[derive(Debug, Serialize)]
pub struct UpdateCreatorApplicationEvent {
    pub application: CreatorApplicationDto,
}

impl UpdateCreatorApplicationEvent {
    pub fn create(app: &CreatorApplication, submitter: &Player) -> Self {
        Self {
            application: CreatorApplicationDto::create(app, submitter),
        }
    }
}
impl_event!(
    UpdateCreatorApplicationEvent,
    "UpdateCreatorApplicationEvent"
);
