use std::collections::HashMap;
use std::sync::Arc;

use chrono::Duration;
use tokio_util::sync::CancellationToken;

use crate::data::Db;
use crate::error::Result;
use crate::events::EventDispatcher;
use crate::players::events::UpdatePlayerPortfolioEvent;
use crate::primitives::{Credits, Id};

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct CalculatePlayerPortfolioOptions {
    pub delay: Duration,
}

impl Default for CalculatePlayerPortfolioOptions {
    fn default() -> Self {
        Self {
            delay: Duration::seconds(60),
        }
    }
}

pub struct CalculatePlayerPortfolioJob {
    options: CalculatePlayerPortfolioOptions,
    db: Db,
    events: Arc<dyn EventDispatcher>,
}

impl CalculatePlayerPortfolioJob {
    pub fn new(
        options: CalculatePlayerPortfolioOptions,
        db: Db,
        events: Arc<dyn EventDispatcher>,
    ) -> Self {
        Self {
            options,
            db,
            events,
        }
    }

    pub async fn run(&self, cancel: CancellationToken) {
        tracing::info!("Started.");
        let delay = self
            .options
            .delay
            .to_std()
            .unwrap_or_else(|_| std::time::Duration::from_secs(60));
        loop {
            if cancel.is_cancelled() {
                break;
            }
            if let Err(err) = self.calculate_all().await {
                tracing::error!(?err, "Error calculating player portfolios");
            }
            tokio::select! {
                _ = cancel.cancelled() => break,
                _ = tokio::time::sleep(delay) => {}
            }
        }
    }

    pub async fn calculate_all(&self) -> Result<()> {
        let creator_values: HashMap<Id, Credits> = self
            .db
            .all_creators()
            .await?
            .into_iter()
            .map(|c| (c.id(), c.value))
            .collect();

        let players = self.db.all_players().await?;

        for mut player in players {
            let guard = match self.db.lock_player(player.id()).await {
                Ok(guard) => guard,
                Err(err) => {
                    tracing::warn!(
                        player_id = player.id(),
                        %err,
                        "skipping portfolio recompute; player is locked"
                    );
                    continue;
                }
            };

            player.transactions = self.db.player_transactions(player.id()).await?;
            let snapshot = player
                .take_portfolio_snapshot(|id| creator_values.get(&id).copied().unwrap_or(0.0));

            self.db
                .commit_portfolio_snapshots(
                    std::slice::from_ref(&player),
                    std::slice::from_ref(&snapshot),
                )
                .await?;

            drop(guard);

            self.events
                .dispatch(&UpdatePlayerPortfolioEvent::create(&snapshot, &player))
                .await?;
        }

        Ok(())
    }
}
