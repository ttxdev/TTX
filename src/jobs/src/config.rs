use chrono::Duration;
use tracing_subscriber::EnvFilter;
use ttx::jobs::{ChatShard, CreatorValuesJobOptions};
use ttx::platforms::twitch::TwitchOAuthOptions;

pub fn init_tracing() {
    tracing_subscriber::fmt()
        .with_env_filter(
            EnvFilter::try_from_default_env().unwrap_or_else(|_| EnvFilter::new("info")),
        )
        .init();
}

pub fn database_url() -> String {
    std::env::var("DATABASE_URL")
        .unwrap_or_else(|_| "postgres://postgres:postgres@localhost:5432/ttx".into())
}

pub fn redis_url() -> Option<String> {
    std::env::var("REDIS_URL").ok()
}

pub fn twitch_options() -> Option<TwitchOAuthOptions> {
    Some(TwitchOAuthOptions {
        client_id: std::env::var("TWITCH_CLIENT_ID").ok()?,
        client_secret: std::env::var("TWITCH_CLIENT_SECRET").ok()?,
        redirect_uri: std::env::var("TWITCH_REDIRECT_URI").unwrap_or_default(),
    })
}

pub fn creator_values_options() -> CreatorValuesJobOptions {
    let secs = std::env::var("CREATOR_VALUES_DELAY_SECS")
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(15);
    CreatorValuesJobOptions {
        delay: Duration::seconds(secs),
        shard: ChatShard::from_env(),
    }
}
