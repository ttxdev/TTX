use std::error::Error;
use std::sync::Arc;

use sqlx::postgres::PgPoolOptions;
use tokio_util::sync::CancellationToken;
use ttx::data::Db;
use ttx::events::EventDispatcher;
use ttx::events::postgres::PgNotifyDispatcher;

use crate::config;

/// Shared infrastructure handed to every job runner.
pub struct Context {
    pub db: Db,
    pub dispatcher: Arc<dyn EventDispatcher>,
    pub database_url: String,
    pub cancel: CancellationToken,
}

impl Context {
    pub async fn init() -> Result<Self, Box<dyn Error>> {
        let database_url = config::database_url();
        let pool = PgPoolOptions::new()
            .max_connections(5)
            .connect(&database_url)
            .await?;
        sqlx::migrate!("../ttx/migrations").run(&pool).await?;

        let cache = ttx::cache::cache_from_env().await;
        let db = Db::with_cache(pool.clone(), cache);
        let dispatcher: Arc<dyn EventDispatcher> = Arc::new(PgNotifyDispatcher::new(pool));

        Ok(Self {
            db,
            dispatcher,
            database_url,
            cancel: CancellationToken::new(),
        })
    }
}
