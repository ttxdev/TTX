use axum::extract::State;
use axum::routing::post;
use axum::{Json, Router};
use serde::Deserialize;
use ttx::primitives::Id;
use ttx::transactions::TransactionAction;
use utoipa::ToSchema;

use crate::auth::AuthPlayer;
use crate::error::ApiResult;
use crate::state::AppState;

pub fn router() -> Router<AppState> {
    Router::new().route("/", post(create))
}

#[derive(Debug, Deserialize, ToSchema)]
pub(crate) struct CreateTransactionDto {
    #[serde(rename = "creator")]
    creator_slug: String,
    action: TransactionAction,
    #[serde(rename = "amount")]
    quantity: u64,
}

#[utoipa::path(
    post, path = "/transactions", operation_id = "PlaceOrder", tag = "transactions",
    request_body = CreateTransactionDto,
    security(("bearer_auth" = [])),
    responses(
        (status = 200, description = "Created transaction id", body = u64),
        (status = 400, description = "Market closed / insufficient funds or shares"),
        (status = 401, description = "Unauthenticated"),
        (status = 404, description = "Player or creator not found"),
    ),
)]
pub(crate) async fn create(
    State(state): State<AppState>,
    auth: AuthPlayer,
    Json(order): Json<CreateTransactionDto>,
) -> ApiResult<Json<Id>> {
    let id = state
        .ttx
        .transactions
        .place_order(auth.id, &order.creator_slug, order.action, order.quantity)
        .await?;
    Ok(Json(id))
}
