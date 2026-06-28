use tracing_subscriber::EnvFilter;
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
        .unwrap_or_else(|_| "postgres://postgres:postgres@localhost/ttx".into())
}

pub fn bind_addr() -> String {
    std::env::var("BIND_ADDR").unwrap_or_else(|_| "0.0.0.0:3000".into())
}

pub fn twitch_options() -> TwitchOAuthOptions {
    TwitchOAuthOptions {
        client_id: std::env::var("TWITCH_CLIENT_ID").unwrap_or_default(),
        client_secret: std::env::var("TWITCH_CLIENT_SECRET").unwrap_or_default(),
        redirect_uri: std::env::var("TWITCH_REDIRECT_URI").unwrap_or_default(),
    }
}
