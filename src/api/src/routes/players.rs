use axum::extract::{Path, Query, State};
use axum::routing::{get, put};
use axum::{Json, Router};
use chrono::Duration;
use serde::Deserialize;
use ttx::dto::pagination::{Order, OrderDirection, PaginatedRequest, PaginationDto};
use ttx::dto::portfolio::{HistoryParams, TimeStep};
use ttx::error::Error as DomainError;
use ttx::lootboxes::dto::LootBoxResultDto;
use ttx::players::dto::PlayerDto;
use ttx::players::{IndexPlayersRequest, PlayerOrderBy};
use utoipa::IntoParams;

use crate::auth::AuthPlayer;
use crate::error::{ApiError, ApiResult};
use crate::routes::history_params;
use crate::state::AppState;

pub fn router() -> Router<AppState> {
    Router::new()
        .route("/", get(index))
        .route("/me", get(get_me))
        .route("/me/lootboxes/{lootbox_id}/open", put(gamba))
        .route("/{username}", get(show))
}

#[derive(Debug, Deserialize, IntoParams)]
#[into_params(parameter_in = Query)]
pub(crate) struct IndexQuery {
    #[serde(default = "default_page")]
    page: i64,
    #[serde(default = "default_limit")]
    limit: i64,
    search: Option<String>,
    #[serde(rename = "orderBy")]
    order_by: Option<PlayerOrderBy>,
    #[serde(rename = "orderDir")]
    order_dir: Option<OrderDirection>,
}

fn default_page() -> i64 {
    1
}

fn default_limit() -> i64 {
    20
}

#[derive(Debug, Deserialize, IntoParams)]
#[into_params(parameter_in = Query)]
pub(crate) struct ShowQuery {
    before: Option<i64>,
}

fn daily() -> HistoryParams {
    HistoryParams {
        step: TimeStep::ThirtyMinute,
        before: Duration::days(1),
    }
}

#[utoipa::path(
    get, path = "/players", operation_id = "GetPlayers", tag = "players",
    params(IndexQuery),
    responses((status = 200, description = "A page of players", body = PaginationDto<PlayerDto>)),
)]
pub(crate) async fn index(
    State(state): State<AppState>,
    Query(query): Query<IndexQuery>,
) -> ApiResult<Json<PaginationDto<PlayerDto>>> {
    let order = query.order_by.map(|by| Order {
        by,
        dir: query.order_dir.unwrap_or(OrderDirection::Ascending),
    });

    let request = IndexPlayersRequest {
        pagination: PaginatedRequest {
            page: query.page,
            limit: query.limit,
            search: query.search,
            order,
        },
        history_params: daily(),
    };

    Ok(Json(state.ttx.players.index(request).await?))
}

#[utoipa::path(
    get, path = "/players/{username}", operation_id = "GetPlayer", tag = "players",
    params(("username" = String, Path, description = "Player username/slug"), ShowQuery),
    responses(
        (status = 200, description = "The player", body = PlayerDto),
        (status = 404, description = "Not found"),
    ),
)]
pub(crate) async fn show(
    State(state): State<AppState>,
    Path(username): Path<String>,
    Query(query): Query<ShowQuery>,
) -> ApiResult<Json<PlayerDto>> {
    let before = query
        .before
        .map_or_else(|| Duration::days(1), Duration::seconds);
    let player = state
        .ttx
        .players
        .find(&username, history_params(before))
        .await?
        .ok_or_else(|| ApiError(DomainError::not_found("Player")))?;
    Ok(Json(player))
}

#[utoipa::path(
    get, path = "/players/me", operation_id = "GetSelf", tag = "players",
    security(("bearer_auth" = [])),
    responses(
        (status = 200, description = "The current player", body = PlayerDto),
        (status = 401, description = "Unauthenticated"),
        (status = 404, description = "Not found"),
    ),
)]
pub(crate) async fn get_me(
    State(state): State<AppState>,
    auth: AuthPlayer,
) -> ApiResult<Json<PlayerDto>> {
    let player = state
        .ttx
        .players
        .find(&auth.slug, daily())
        .await?
        .ok_or_else(|| ApiError(DomainError::not_found("Player")))?;
    Ok(Json(player))
}

#[utoipa::path(
    put, path = "/players/me/lootboxes/{lootbox_id}/open", operation_id = "Gamba", tag = "players",
    params(("lootbox_id" = u64, Path, description = "Lootbox id")),
    security(("bearer_auth" = [])),
    responses(
        (status = 200, description = "The lootbox result", body = LootBoxResultDto),
        (status = 400, description = "Already opened / no eligible creators"),
        (status = 401, description = "Unauthenticated"),
        (status = 404, description = "Player or lootbox not found"),
    ),
)]
pub(crate) async fn gamba(
    State(state): State<AppState>,
    auth: AuthPlayer,
    Path(lootbox_id): Path<u64>,
) -> ApiResult<Json<LootBoxResultDto>> {
    let result = state
        .ttx
        .lootboxes
        .open_loot_box(auth.id, lootbox_id)
        .await?;
    Ok(Json(result))
}
