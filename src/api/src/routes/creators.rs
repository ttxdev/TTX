use axum::extract::{Path, Query, State};
use axum::http::StatusCode;
use axum::response::{IntoResponse, Response};
use axum::routing::get;
use axum::{Json, Router};
use chrono::Duration;
use serde::Deserialize;
use ttx::creators::dto::CreatorDto;
use ttx::creators::opt_outs::dto::CreatorOptOutDto;
use ttx::creators::{CreatorOrderBy, IndexCreatorsRequest, OnboardRequest};
use ttx::dto::pagination::{Order, OrderDirection, PaginatedRequest, PaginationDto};
use ttx::dto::portfolio::{HistoryParams, TimeStep};
use ttx::error::Error as DomainError;
use ttx::platforms::Platform;
use utoipa::IntoParams;

use crate::auth::AuthPlayer;
use crate::error::{ApiError, ApiResult};
use crate::routes::history_params;
use crate::state::AppState;

pub fn router() -> Router<AppState> {
    Router::new()
        .route("/", get(index).post(create))
        .route("/{slug}", get(show).delete(opt_out))
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
    order_by: Option<CreatorOrderBy>,
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

#[derive(Debug, Deserialize, IntoParams)]
#[into_params(parameter_in = Query)]
pub(crate) struct CreateQuery {
    username: String,
    ticker: String,
}

#[derive(Debug, Deserialize, IntoParams)]
#[into_params(parameter_in = Query)]
pub(crate) struct OptOutQuery {
    reason: Option<String>,
}

#[utoipa::path(
    get, path = "/creators", operation_id = "GetCreators", tag = "creators",
    params(IndexQuery),
    responses((status = 200, description = "A page of creators", body = PaginationDto<CreatorDto>)),
)]
pub(crate) async fn index(
    State(state): State<AppState>,
    Query(query): Query<IndexQuery>,
) -> ApiResult<Json<PaginationDto<CreatorDto>>> {
    let order = query.order_by.map(|by| Order {
        by,
        dir: query.order_dir.unwrap_or(OrderDirection::Ascending),
    });

    let request = IndexCreatorsRequest {
        pagination: PaginatedRequest {
            page: query.page,
            limit: query.limit,
            search: query.search,
            order,
        },
        min_value: None,
        history_params: HistoryParams {
            step: TimeStep::FifteenMinute,
            before: Duration::days(1),
        },
    };

    Ok(Json(state.ttx.creators.index(request).await?))
}

#[utoipa::path(
    get, path = "/creators/{slug}", operation_id = "GetCreator", tag = "creators",
    params(("slug" = String, Path, description = "Creator slug"), ShowQuery),
    responses(
        (status = 200, description = "The creator", body = CreatorDto),
        (status = 404, description = "Not found"),
    ),
)]
pub(crate) async fn show(
    State(state): State<AppState>,
    Path(slug): Path<String>,
    Query(query): Query<ShowQuery>,
) -> ApiResult<Json<CreatorDto>> {
    let before = query
        .before
        .map_or_else(|| Duration::days(1), Duration::seconds);
    let creator = state
        .ttx
        .creators
        .find(&slug, history_params(before))
        .await?
        .ok_or_else(|| ApiError(DomainError::not_found("Creator")))?;
    Ok(Json(creator))
}

#[utoipa::path(
    post, path = "/creators", operation_id = "CreateCreator", tag = "creators",
    params(CreateQuery),
    security(("bearer_auth" = [])),
    responses(
        (status = 200, description = "Created creator id", body = u64),
        (status = 400, description = "Validation failed"),
        (status = 403, description = "Not an admin"),
    ),
)]
pub(crate) async fn create(
    State(state): State<AppState>,
    auth: AuthPlayer,
    Query(query): Query<CreateQuery>,
) -> Response {
    if !auth.is_admin() {
        return StatusCode::FORBIDDEN.into_response();
    }

    let request = OnboardRequest {
        username: Some(query.username),
        platform_id: None,
        platform: Platform::Twitch,
        ticker: query.ticker,
    };

    match state.ttx.creators.onboard(request).await {
        Ok(id) => Json(id).into_response(),
        Err(err) => ApiError(err).into_response(),
    }
}

#[utoipa::path(
    delete, path = "/creators/{slug}", operation_id = "OptOutCreator", tag = "creators",
    params(("slug" = String, Path, description = "Creator slug"), OptOutQuery),
    security(("bearer_auth" = [])),
    responses(
        (status = 200, description = "The opt-out record", body = CreatorOptOutDto),
        (status = 401, description = "Not the creator"),
        (status = 404, description = "Not found"),
    ),
)]
pub(crate) async fn opt_out(
    State(state): State<AppState>,
    auth: AuthPlayer,
    Path(slug): Path<String>,
    Query(query): Query<OptOutQuery>,
) -> Response {
    if !auth.is_admin() {
        match state.ttx.creators.is_player(&slug, auth.id).await {
            Ok(true) => {}
            Ok(false) => return StatusCode::UNAUTHORIZED.into_response(),
            Err(err) => return ApiError(err).into_response(),
        }
    }

    match state
        .ttx
        .creators
        .opt_out(&slug, query.reason.unwrap_or_default())
        .await
    {
        Ok(dto) => Json::<CreatorOptOutDto>(dto).into_response(),
        Err(err) => ApiError(err).into_response(),
    }
}
