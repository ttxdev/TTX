use axum::extract::{Query, State};
use axum::http::StatusCode;
use axum::response::{IntoResponse, Response};
use axum::routing::get;
use axum::{Json, Router};
use serde::{Deserialize, Serialize};
use ttx::platforms::Platform;
use utoipa::{IntoParams, ToSchema};

use crate::auth;
use crate::error::ApiError;
use crate::state::AppState;

pub fn router() -> Router<AppState> {
    Router::new()
        .route("/login", get(login))
        .route("/twitch/callback", get(twitch_callback))
}

#[derive(Debug, Serialize, ToSchema)]
pub(crate) struct TokenDto {
    access_token: String,
}

#[derive(Debug, Deserialize, IntoParams)]
#[into_params(parameter_in = Query)]
pub(crate) struct CallbackQuery {
    code: String,
    // Accepted but not validated; the C# state-key check was a TODO there too.
    #[allow(dead_code)]
    state: Option<String>,
}

#[utoipa::path(
    get, path = "/sessions/login", operation_id = "GetLoginUrl", tag = "sessions",
    responses((status = 200, description = "The OAuth login URL", body = String)),
)]
pub(crate) async fn login(State(state): State<AppState>) -> String {
    // Plain text (not `Json<String>`): the OpenAPI response is `text/plain`, and
    // a JSON-encoded body would wrap the URL in quotes, breaking the client's
    // redirect.
    state.twitch.get_login_url()
}

#[utoipa::path(
    get, path = "/sessions/twitch/callback", operation_id = "TwitchCallback", tag = "sessions",
    params(CallbackQuery),
    responses(
        (status = 200, description = "A signed session token", body = TokenDto),
        (status = 400, description = "Authentication failed"),
    ),
)]
pub(crate) async fn twitch_callback(
    State(state): State<AppState>,
    Query(query): Query<CallbackQuery>,
) -> Response {
    let player = match state
        .ttx
        .players
        .authenticate(Platform::Twitch, &query.code)
        .await
    {
        Ok(player) => player,
        Err(err) => return ApiError(err).into_response(),
    };

    match auth::issue(&state.jwt, &player) {
        Ok(token) => Json(TokenDto {
            access_token: token,
        })
        .into_response(),
        Err(_) => StatusCode::INTERNAL_SERVER_ERROR.into_response(),
    }
}
