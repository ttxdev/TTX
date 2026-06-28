//! Players feature module.

pub mod data;
pub mod dto;
pub mod events;
pub mod model;

pub use model::{Player, PlayerType, PortfolioSnapshot};

use std::collections::HashMap;
use std::sync::Arc;

use serde::Deserialize;

use crate::creators::Creator;
use crate::data::Db;
use crate::dto::pagination::{Order, OrderDirection, PaginatedRequest, PaginationDto};
use crate::dto::portfolio::HistoryParams;
use crate::error::{Error, Result};
use crate::events::EventDispatcher;
use crate::platforms::{Platform, PlatformUser, PlatformUserService};
use crate::primitives::Id;
use crate::transactions::TransactionAction;

use dto::{PlayerDto, PlayerPartialDto};
use events::CreatePlayerEvent;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Deserialize, utoipa::ToSchema)]
pub enum PlayerOrderBy {
    Name,
    Credits,
    Portfolio,
}

#[derive(Debug)]
pub struct IndexPlayersRequest {
    pub pagination: PaginatedRequest<PlayerOrderBy>,
    pub history_params: HistoryParams,
}

#[derive(Debug, Deserialize, utoipa::ToSchema)]
pub struct PlaceOrderRequest {
    pub actor_id: Id,
    pub creator_slug: String,
    pub quantity: u64,
    pub action: TransactionAction,
}

fn player_order_clause(order: Option<Order<PlayerOrderBy>>) -> String {
    let order = order.unwrap_or(Order {
        by: PlayerOrderBy::Name,
        dir: OrderDirection::Ascending,
    });
    let dir = match order.dir {
        OrderDirection::Ascending => "ASC",
        OrderDirection::Descending => "DESC",
    };
    match order.by {
        PlayerOrderBy::Name => "name ASC".to_string(),
        PlayerOrderBy::Credits => format!("credits {dir}, name ASC"),
        PlayerOrderBy::Portfolio => format!("portfolio {dir}, name ASC"),
    }
}

pub struct PlayerService {
    db: Db,
    events: Arc<dyn EventDispatcher>,
    platform_users: HashMap<Platform, Arc<dyn PlatformUserService>>,
}

impl PlayerService {
    pub fn new(
        db: Db,
        events: Arc<dyn EventDispatcher>,
        platform_users: HashMap<Platform, Arc<dyn PlatformUserService>>,
    ) -> Self {
        Self {
            db,
            events,
            platform_users,
        }
    }

    pub async fn index(&self, request: IndexPlayersRequest) -> Result<PaginationDto<PlayerDto>> {
        let limit = request.pagination.limit.min(100);
        let page = request.pagination.page.max(1);
        let offset = (page - 1) * limit;
        let search = request.pagination.search.as_deref();
        let order_clause = player_order_clause(request.pagination.order);

        let total = self.db.count_players(search).await?;
        let mut players = self
            .db
            .index_players(search, &order_clause, offset, limit)
            .await?;

        let mut history = self
            .db
            .player_history(
                &players,
                &request.history_params.step,
                &request.history_params.before,
            )
            .await?;
        for player in &mut players {
            if let Some(snapshots) = history.remove(&player.id()) {
                player.history = snapshots;
            }
        }

        // Index does not load transactions/lootboxes/shares.
        let creators = HashMap::new();
        let data = players
            .iter()
            .map(|p| PlayerDto::create(p, &creators))
            .collect();

        Ok(PaginationDto { data, total })
    }

    pub async fn find(
        &self,
        slug: &str,
        history_params: HistoryParams,
    ) -> Result<Option<PlayerDto>> {
        let Some(mut player) = self.db.player_by_slug(slug).await? else {
            return Ok(None);
        };

        player.transactions = self.db.player_transactions(player.id()).await?;
        player.lootboxes = self.db.player_lootboxes(player.id(), true).await?;

        let mut creator_ids: Vec<Id> = player.transactions.iter().map(|t| t.creator_id).collect();
        creator_ids.extend(player.lootboxes.iter().filter_map(|l| l.result_id));
        let creators: HashMap<Id, Creator> = self
            .db
            .creators_by_ids(&creator_ids)
            .await?
            .into_iter()
            .map(|c| (c.id(), c))
            .collect();

        let mut history = self
            .db
            .player_history(
                std::slice::from_ref(&player),
                &history_params.step,
                &history_params.before,
            )
            .await?;
        if let Some(snapshots) = history.remove(&player.id()) {
            player.history = snapshots;
        }

        Ok(Some(PlayerDto::create(&player, &creators)))
    }

    pub async fn authenticate(
        &self,
        platform: Platform,
        oauth_code: &str,
    ) -> Result<PlayerPartialDto> {
        let platform_service = self
            .platform_users
            .get(&platform)
            .ok_or_else(|| Error::not_found("PlatformUserService"))?;

        let Some(user) = platform_service.resolve_by_oauth(oauth_code).await? else {
            return Err(Error::not_found("PlatformUser"));
        };

        self.onboard(platform, user).await
    }

    pub async fn onboard(
        &self,
        platform: Platform,
        user: PlatformUser,
    ) -> Result<PlayerPartialDto> {
        let platform_id_str = user.id.to_string();
        if let Some(mut player) = self
            .db
            .player_by_platform(platform, &platform_id_str)
            .await?
        {
            if player.user.sync(&user) {
                self.db.update_player_profile(&player).await?;
            }
            return Ok(PlayerPartialDto::create(&player));
        }

        let credits = self.db.average_creator_value().await?;
        let mut player = model::Player::create(&user, Some(credits));
        self.db.insert_player(&mut player).await?;
        self.events
            .dispatch(&CreatePlayerEvent::create(&player, &HashMap::new()))
            .await?;

        Ok(PlayerPartialDto::create(&player))
    }
}
