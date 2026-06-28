use std::collections::HashMap;
use std::sync::Arc;

use sqlx::PgPool;

use crate::cache::Cache;
use crate::creators::CreatorService;
use crate::data::Db;
use crate::events::EventDispatcher;
use crate::factories::ChatMonitorFactory;
use crate::jobs::ChatMonitorAdapter;
use crate::lootboxes::LootBoxService;
use crate::options::RandomOptions;
use crate::platforms::Platform;
use crate::platforms::PlatformUserService;
use crate::players::PlayerService;
use crate::transactions::TransactionService;

pub struct Ttx {
    pub db: Db,
    pub creators: CreatorService,
    pub players: PlayerService,
    pub transactions: TransactionService,
    pub lootboxes: LootBoxService,
    pub chat_monitors: Arc<ChatMonitorFactory>,
}

impl Ttx {
    pub fn new(
        pool: PgPool,
        events: Arc<dyn EventDispatcher>,
        platform_users: HashMap<Platform, Arc<dyn PlatformUserService>>,
        chat_adapters: HashMap<Platform, Arc<dyn ChatMonitorAdapter>>,
        random: RandomOptions,
        cache: Arc<dyn Cache>,
    ) -> Self {
        let db = Db::with_cache(pool, cache);
        Self {
            creators: CreatorService::new(db.clone(), events.clone(), platform_users.clone()),
            players: PlayerService::new(db.clone(), events.clone(), platform_users),
            transactions: TransactionService::new(db.clone(), events.clone()),
            lootboxes: LootBoxService::new(db.clone(), events, random),
            chat_monitors: Arc::new(ChatMonitorFactory::new(chat_adapters)),
            db,
        }
    }
}
