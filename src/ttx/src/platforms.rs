pub mod shared;
pub mod twitch;

use serde::{Deserialize, Serialize};
pub use shared::PlatformUserService;
pub type PlatformId = String;

#[derive(
    Debug, Clone, Copy, PartialEq, Eq, Hash, Default, Serialize, Deserialize, utoipa::ToSchema,
)]
pub enum Platform {
    #[default]
    Twitch,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct PlatformUser {
    pub id: PlatformId,
    pub username: String,
    pub display_name: String,
    pub avatar_url: String,
}

impl Platform {
    pub fn url(&self, slug: &str) -> String {
        match self {
            Platform::Twitch => format!("https://twitch.tv/{slug}"),
        }
    }
}
