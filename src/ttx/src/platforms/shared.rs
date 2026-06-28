use crate::error::Result;
use crate::platforms::{PlatformId, PlatformUser};
use async_trait::async_trait;

#[async_trait]
pub trait PlatformUserService: Send + Sync {
    async fn get_user_by_username(&self, username: &str) -> Result<Option<PlatformUser>>;
    async fn get_user_by_id(&self, id: PlatformId) -> Result<Option<PlatformUser>>;
    async fn resolve_by_oauth(&self, code: &str) -> Result<Option<PlatformUser>>;
    fn get_login_url(&self) -> String;
}
