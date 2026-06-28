use crate::platforms::{Platform, PlatformId, PlatformUser};
use crate::primitives::Meta;

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct User {
    pub meta: Meta,
    pub name: String,
    pub slug: String,
    pub platform_id: PlatformId,
    pub platform: Platform,
    pub avatar_url: String,
}

impl User {
    pub fn sync(&mut self, user: &PlatformUser) -> bool {
        let mut changed = false;

        if user.display_name != self.name {
            self.name = user.display_name.clone();
            changed = true;
        }

        if user.username != self.slug {
            self.slug = user.username.clone();
            changed = true;
        }

        if user.avatar_url != self.avatar_url {
            self.avatar_url = user.avatar_url.clone();
            changed = true;
        }

        changed
    }
}
