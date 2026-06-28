use std::sync::Arc;

use async_trait::async_trait;
use jobs::Plugins;
use ttx::jobs::{KeywordMessageAnalyzer, MessageAnalyzer, StatsProcessor, SumStatsProcessor};

struct DefaultPlugins;

#[async_trait]
impl Plugins for DefaultPlugins {
    fn analyzer(&self) -> Arc<dyn MessageAnalyzer> {
        Arc::new(KeywordMessageAnalyzer)
    }

    async fn processor(&self, _redis_url: &str) -> Arc<dyn StatsProcessor> {
        Arc::new(SumStatsProcessor)
    }
}

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    jobs::run(DefaultPlugins).await
}
