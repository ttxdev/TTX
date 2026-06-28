use std::collections::HashMap;
use std::error::Error;
use std::sync::Arc;

use sqlx::postgres::PgPoolOptions;
use ttx::di::Ttx;
use ttx::events::EventDispatcher;
use ttx::jobs::ChatMonitorAdapter;
use ttx::options::RandomOptions;
use ttx::platforms::twitch::TwitchUserService;
use ttx::platforms::{Platform, PlatformUserService};

use crate::auth::JwtConfig;
use crate::config;
use crate::events::EventBroadcaster;

#[derive(Clone)]
pub struct AppState {
    pub ttx: Arc<Ttx>,
    pub jwt: Arc<JwtConfig>,
    pub events: EventBroadcaster,
    pub twitch: Arc<dyn PlatformUserService>,
}

impl AppState {
    pub async fn build(database_url: &str) -> Result<Self, Box<dyn Error>> {
        let pool = PgPoolOptions::new()
            .max_connections(10)
            .connect(database_url)
            .await?;
        sqlx::migrate!("../ttx/migrations").run(&pool).await?;

        let events = EventBroadcaster::new();
        let dispatcher: Arc<dyn EventDispatcher> = Arc::new(events.clone());

        let twitch: Arc<dyn PlatformUserService> =
            Arc::new(TwitchUserService::new(config::twitch_options()));

        let mut platform_users: HashMap<Platform, Arc<dyn PlatformUserService>> = HashMap::new();
        platform_users.insert(Platform::Twitch, twitch.clone());
        let chat_adapters: HashMap<Platform, Arc<dyn ChatMonitorAdapter>> = HashMap::new();

        let cache = ttx::cache::cache_from_env().await;

        let ttx = Ttx::new(
            pool,
            dispatcher,
            platform_users,
            chat_adapters,
            RandomOptions::default(),
            cache,
        );

        Ok(Self {
            ttx: Arc::new(ttx),
            jwt: Arc::new(JwtConfig::from_env()),
            events,
            twitch,
        })
    }
}
