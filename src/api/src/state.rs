use std::sync::Arc;

use ttx::di::Ttx;
use ttx::platforms::PlatformUserService;

use crate::auth::JwtConfig;
use crate::events::EventBroadcaster;

#[derive(Clone)]
pub struct AppState {
    pub ttx: Arc<Ttx>,
    pub jwt: Arc<JwtConfig>,
    pub events: EventBroadcaster,
    pub twitch: Arc<dyn PlatformUserService>,
}
