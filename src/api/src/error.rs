use axum::Json;
use axum::http::StatusCode;
use axum::response::{IntoResponse, Response};
use serde_json::json;
use ttx::error::Error as DomainError;

pub struct ApiError(pub DomainError);

impl From<DomainError> for ApiError {
    fn from(value: DomainError) -> Self {
        Self(value)
    }
}

impl IntoResponse for ApiError {
    fn into_response(self) -> Response {
        let (status, body) = match &self.0 {
            DomainError::NotFound(name) => (
                StatusCode::NOT_FOUND,
                json!({ "error": "NotFound", "message": format!("{name} not found") }),
            ),
            DomainError::InvalidRequest(failures) => (
                StatusCode::BAD_REQUEST,
                json!({
                    "error": "InvalidRequest",
                    "failures": failures
                        .iter()
                        .map(|f| json!({
                            "property": f.property,
                            "code": f.code,
                            "message": f.message,
                        }))
                        .collect::<Vec<_>>(),
                }),
            ),
            DomainError::Database(msg) => (
                StatusCode::INTERNAL_SERVER_ERROR,
                json!({ "error": "Database", "message": msg }),
            ),
            DomainError::External(msg) => (
                StatusCode::BAD_GATEWAY,
                json!({ "error": "External", "message": msg }),
            ),
            DomainError::InvalidAction(msg) | DomainError::InvalidValueObject(msg) => (
                StatusCode::BAD_REQUEST,
                json!({ "error": "InvalidAction", "message": msg }),
            ),
        };

        (status, Json(body)).into_response()
    }
}

pub type ApiResult<T> = Result<T, ApiError>;
