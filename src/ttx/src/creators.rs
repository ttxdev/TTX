pub mod applications;
pub mod data;
pub mod dto;
pub mod events;
pub mod model;
pub mod opt_outs;

pub use model::{Creator, StreamStatus, Vote};

use std::collections::HashMap;
use std::sync::Arc;

use serde::Deserialize;
use validator::{Validate, ValidationError, ValidationErrors};

use crate::data::Db;
use crate::dto::pagination::{Order, OrderDirection, PaginatedRequest, PaginationDto};
use crate::dto::portfolio::HistoryParams;
use crate::error::{Error, Result, ValidationFailure};
use crate::events::EventDispatcher;
use crate::platforms::{Platform, PlatformId, PlatformUserService};
use crate::primitives::Id;

use dto::CreatorDto;
use events::CreateCreatorEvent;
use opt_outs::CreatorOptOut;
use opt_outs::dto::CreatorOptOutDto;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Deserialize, utoipa::ToSchema)]
pub enum CreatorOrderBy {
    Name,
    Value,
    IsLive,
}

#[derive(Debug)]
pub struct IndexCreatorsRequest {
    pub pagination: PaginatedRequest<CreatorOrderBy>,
    pub min_value: Option<f64>,
    pub history_params: HistoryParams,
}

#[derive(Debug, Deserialize, Validate)]
#[validate(schema(function = "validate_identifier"))]
pub struct OnboardRequest {
    pub username: Option<String>,
    pub platform_id: Option<PlatformId>,
    pub platform: Platform,
    pub ticker: String,
}

fn validate_identifier(req: &OnboardRequest) -> std::result::Result<(), ValidationError> {
    if req.username.is_none() && req.platform_id.is_none() {
        return Err(ValidationError::new("REQUIRED").with_message("is required".into()));
    }
    Ok(())
}

fn collect_failures(errors: &ValidationErrors) -> Vec<ValidationFailure> {
    let mut failures = Vec::new();
    for (field, errs) in errors.field_errors() {
        for err in errs {
            failures.push(ValidationFailure {
                property: field.to_string(),
                code: err.code.to_string(),
                message: err
                    .message
                    .as_ref()
                    .map(|m| m.to_string())
                    .unwrap_or_default(),
            });
        }
    }
    failures
}

fn creator_order_clause(order: Option<Order<CreatorOrderBy>>) -> String {
    let order = order.unwrap_or(Order {
        by: CreatorOrderBy::Name,
        dir: OrderDirection::Ascending,
    });
    let dir = match order.dir {
        OrderDirection::Ascending => "ASC",
        OrderDirection::Descending => "DESC",
    };
    match order.by {
        CreatorOrderBy::Name => format!("name {dir}"),
        CreatorOrderBy::Value => format!("value {dir}, name ASC"),
        CreatorOrderBy::IsLive => format!("stream_is_live {dir}, name ASC"),
    }
}

pub struct CreatorService {
    db: Db,
    events: Arc<dyn EventDispatcher>,
    platform_users: HashMap<Platform, Arc<dyn PlatformUserService>>,
}

impl CreatorService {
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

    pub async fn index(&self, request: IndexCreatorsRequest) -> Result<PaginationDto<CreatorDto>> {
        let limit = request.pagination.limit.min(100);
        let page = request.pagination.page.max(1);
        let offset = (page - 1) * limit;
        let search = request.pagination.search.as_deref();
        let order_clause = creator_order_clause(request.pagination.order);

        let total = self.db.count_creators(request.min_value, search).await?;
        let mut creators = self
            .db
            .index_creators(request.min_value, search, &order_clause, offset, limit)
            .await?;

        let mut history = self
            .db
            .creator_history(
                &creators,
                request.history_params.step,
                request.history_params.before,
            )
            .await?;
        for creator in &mut creators {
            if let Some(votes) = history.remove(&creator.id()) {
                creator.history = votes;
            }
        }

        let players = HashMap::new();
        let data = creators
            .iter()
            .map(|c| CreatorDto::create(c, &players))
            .collect();

        Ok(PaginationDto { data, total })
    }

    pub async fn find(
        &self,
        slug: &str,
        history_params: HistoryParams,
    ) -> Result<Option<CreatorDto>> {
        let Some(mut creator) = self.db.creator_by_slug(slug).await? else {
            return Ok(None);
        };

        creator.transactions = self.db.creator_transactions(creator.id()).await?;

        let player_ids: Vec<Id> = creator.transactions.iter().map(|t| t.player_id).collect();
        let players: HashMap<Id, _> = self
            .db
            .players_by_ids(&player_ids)
            .await?
            .into_iter()
            .map(|p| (p.id(), p))
            .collect();

        let mut history = self
            .db
            .creator_history(
                std::slice::from_ref(&creator),
                history_params.step,
                history_params.before,
            )
            .await?;
        if let Some(votes) = history.remove(&creator.id()) {
            creator.history = votes;
        }

        Ok(Some(CreatorDto::create(&creator, &players)))
    }

    pub async fn is_player(&self, creator_slug: &str, player_id: Id) -> Result<bool> {
        let Some(creator) = self.db.creator_by_slug(creator_slug).await? else {
            return Ok(false);
        };
        let Some(player) = self.db.player_by_id(player_id).await? else {
            return Ok(false);
        };
        Ok(creator.user.platform == player.user.platform
            && creator.user.platform_id == player.user.platform_id)
    }

    pub async fn opt_out(&self, slug: &str, reason: String) -> Result<CreatorOptOutDto> {
        let Some(creator) = self.db.creator_by_slug(slug).await? else {
            return Err(Error::not_found("Creator"));
        };

        let mut opt = CreatorOptOut::create(&creator, reason);
        self.db.insert_creator_opt_out(&mut opt).await?;
        self.db.delete_creator(creator.id()).await?;

        Ok(CreatorOptOutDto::create(&opt))
    }

    pub async fn onboard(&self, request: OnboardRequest) -> Result<Id> {
        let mut failures = match request.validate() {
            Ok(()) => Vec::new(),
            Err(errors) => collect_failures(&errors),
        };
        if self.db.ticker_exists(&request.ticker).await? {
            failures.push(ValidationFailure {
                property: "ticker".to_string(),
                code: "UNIQUE".to_string(),
                message: "must be unique".to_string(),
            });
        }
        if !failures.is_empty() {
            return Err(Error::InvalidRequest(failures));
        }

        let platform_service = self
            .platform_users
            .get(&request.platform)
            .ok_or_else(|| Error::not_found("PlatformUserService"))?;

        let user = if let Some(username) = &request.username {
            platform_service.get_user_by_username(username).await?
        } else if let Some(platform_id) = request.platform_id {
            platform_service.get_user_by_id(platform_id).await?
        } else {
            None
        };

        let Some(user) = user else {
            return Err(Error::not_found("PlatformUser"));
        };

        let platform_id_str = user.id.to_string();
        if let Some(mut creator) = self
            .db
            .creator_by_platform(request.platform, &platform_id_str)
            .await?
        {
            if creator.user.sync(&user) {
                self.db.update_creator_profile(&creator).await?;
            }
            return Ok(creator.id());
        }

        if self
            .db
            .creator_opt_out_exists(request.platform, &platform_id_str)
            .await?
        {
            return Err(Error::InvalidAction("Creator has opted out".to_string()));
        }

        let mut creator = model::Creator::create(&user, request.ticker, request.platform);
        self.db.insert_creator(&mut creator).await?;
        self.events
            .dispatch(&CreateCreatorEvent::create(&creator))
            .await?;

        Ok(creator.id())
    }
}
