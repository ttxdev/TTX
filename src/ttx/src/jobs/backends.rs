use std::collections::HashMap;

use async_trait::async_trait;
use redis::AsyncCommands;
use redis::aio::ConnectionManager;

use crate::creators::Creator;
use crate::error::{Error, Result};
use crate::jobs::{CreatorStats, CreatorStatsRepository, MessageAnalyzer, StatsProcessor};

const STATS_KEY: &str = "creator_stats";

fn ext(e: impl std::fmt::Display) -> Error {
    Error::External(e.to_string())
}

#[derive(Clone)]
pub struct RedisCreatorStatsRepository {
    conn: ConnectionManager,
}

impl RedisCreatorStatsRepository {
    pub async fn connect(url: &str) -> Result<Self> {
        let client = redis::Client::open(url).map_err(ext)?;
        let conn = ConnectionManager::new(client).await.map_err(ext)?;
        Ok(Self { conn })
    }
}

#[async_trait]
impl CreatorStatsRepository for RedisCreatorStatsRepository {
    async fn get_by_creator(&self, slug: &str) -> Result<CreatorStats> {
        let mut conn = self.conn.clone();
        let data: Option<String> = conn.hget(STATS_KEY, slug).await.map_err(ext)?;
        match data {
            Some(json) => serde_json::from_str(&json).map_err(ext),
            None => Ok(CreatorStats::new(slug.to_string())),
        }
    }

    async fn set_by_creator(&self, slug: &str, stats: CreatorStats) -> Result<()> {
        let mut conn = self.conn.clone();
        let json = serde_json::to_string(&stats).map_err(ext)?;
        let _: () = conn.hset(STATS_KEY, slug, json).await.map_err(ext)?;
        Ok(())
    }

    async fn get_all(&self, clear: bool) -> Result<Vec<CreatorStats>> {
        let mut conn = self.conn.clone();
        let raw: HashMap<String, String> = conn.hgetall(STATS_KEY).await.map_err(ext)?;
        let mut stats = Vec::with_capacity(raw.len());
        for json in raw.values() {
            stats.push(serde_json::from_str(json).map_err(ext)?);
        }
        if clear {
            let _: () = conn.del(STATS_KEY).await.map_err(ext)?;
        }
        Ok(stats)
    }

    async fn clear(&self, slugs: &[String]) -> Result<()> {
        if slugs.is_empty() {
            return Ok(());
        }
        let mut conn = self.conn.clone();
        let _: () = conn.hdel(STATS_KEY, slugs).await.map_err(ext)?;
        Ok(())
    }
}

pub struct KeywordMessageAnalyzer;

#[async_trait]
impl MessageAnalyzer for KeywordMessageAnalyzer {
    async fn analyze(&self, content: &str) -> Result<f64> {
        if content.contains("+2") {
            return Ok(2.0);
        }
        if content.contains("-2") {
            return Ok(-2.0);
        }
        Ok(0.0)
    }
}

pub struct SumStatsProcessor;

#[async_trait]
impl StatsProcessor for SumStatsProcessor {
    async fn process(&self, _creator: &Creator, stats: Option<&CreatorStats>) -> Result<f64> {
        Ok(stats.map_or(0.0, |s| s.positive + s.negative))
    }
}
