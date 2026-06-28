use std::sync::Arc;

use ttx::jobs::{MessageAnalyzer, StatsProcessor};

#[cfg(feature = "private")]
pub fn make_analyzer() -> Arc<dyn MessageAnalyzer> {
    Arc::new(private::VaderMessageAnalyzer::new())
}

#[cfg(not(feature = "private"))]
pub fn make_analyzer() -> Arc<dyn MessageAnalyzer> {
    Arc::new(ttx::jobs::KeywordMessageAnalyzer)
}

#[cfg(feature = "private")]
pub async fn make_processor(redis_url: &str) -> Arc<dyn StatsProcessor> {
    match private::RedisCreatorBaselineRepository::connect(redis_url).await {
        Ok(baselines) => Arc::new(private::BaselineStatsProcessor::new(Arc::new(baselines))),
        Err(err) => {
            tracing::error!(?err, "baseline store unavailable, using sum processor");
            Arc::new(ttx::jobs::SumStatsProcessor)
        }
    }
}

#[cfg(not(feature = "private"))]
pub async fn make_processor(_redis_url: &str) -> Arc<dyn StatsProcessor> {
    Arc::new(ttx::jobs::SumStatsProcessor)
}
