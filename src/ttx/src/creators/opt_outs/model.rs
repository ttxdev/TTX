use crate::creators::Creator;
use crate::platforms::{Platform, PlatformId};
use crate::primitives::{Id, Meta};

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct CreatorOptOut {
    pub meta: Meta,
    pub platform_id: PlatformId,
    pub platform: Platform,
    pub reason: String,
}

impl CreatorOptOut {
    pub fn id(&self) -> Id {
        self.meta.id
    }

    pub fn create(creator: &Creator, reason: String) -> Self {
        Self {
            meta: Meta::default(),
            platform_id: creator.user.platform_id.clone(),
            platform: creator.user.platform,
            reason,
        }
    }
}
