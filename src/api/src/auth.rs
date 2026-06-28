use axum::extract::FromRequestParts;
use axum::http::StatusCode;
use axum::http::header::AUTHORIZATION;
use axum::http::request::Parts;
use axum::response::{IntoResponse, Response};
use chrono::{Duration, Utc};
use jsonwebtoken::{Algorithm, DecodingKey, EncodingKey, Header, Validation, decode, encode};
use serde::{Deserialize, Serialize};
use ttx::players::PlayerType;
use ttx::players::dto::PlayerPartialDto;

use crate::state::AppState;

#[derive(Clone)]
pub struct JwtConfig {
    encoding: EncodingKey,
    decoding: DecodingKey,
    issuer: String,
    audience: String,
    expires: Duration,
    validation: Validation,
}

impl JwtConfig {
    pub fn new(secret: &str, issuer: String, audience: String, expires: Duration) -> Self {
        let mut validation = Validation::new(Algorithm::HS256);
        validation.set_issuer(std::slice::from_ref(&issuer));
        validation.set_audience(std::slice::from_ref(&audience));
        Self {
            encoding: EncodingKey::from_secret(secret.as_bytes()),
            decoding: DecodingKey::from_secret(secret.as_bytes()),
            issuer,
            audience,
            expires,
            validation,
        }
    }

    /// Builds config from the environment.
    ///
    /// `JWT_KEY` is required. To avoid signing tokens with a publicly-known
    /// secret, an unset key is only tolerated outside production (with a loud
    /// warning); when `APP_ENV=production` a missing key aborts startup.
    pub fn from_env() -> Self {
        let secret = match std::env::var("JWT_KEY") {
            Ok(key) => key,
            Err(_) => {
                if std::env::var("APP_ENV").as_deref() == Ok("production") {
                    panic!("JWT_KEY must be set in production");
                }
                tracing::warn!(
                    "JWT_KEY is not set; using an insecure development default. \
                     Set JWT_KEY before deploying."
                );
                "dev-secret-change-me".to_string()
            }
        };
        let issuer = std::env::var("JWT_ISSUER").unwrap_or_else(|_| "api.ttx.gg".into());
        let audience = std::env::var("JWT_AUDIENCE").unwrap_or_else(|_| "ttx.gg".into());
        let days = std::env::var("JWT_EXPIRES_DAYS")
            .ok()
            .and_then(|v| v.parse().ok())
            .unwrap_or(7);
        Self::new(&secret, issuer, audience, Duration::days(days))
    }
}

#[derive(Debug, Serialize)]
struct Claims<'a> {
    sub: &'a str,
    name: &'a str,
    given_name: &'a str,
    role: &'a str,
    avatar_url: &'a str,
    updated_at: &'a str,
    iss: &'a str,
    aud: &'a str,
    exp: i64,
}

/// The subset of claims read back on the request path. Owned because
/// `jsonwebtoken::decode` requires `DeserializeOwned`.
#[derive(Debug, Deserialize)]
struct DecodedClaims {
    sub: String,
    name: String,
    role: String,
}

pub fn issue(
    jwt: &JwtConfig,
    player: &PlayerPartialDto,
) -> Result<String, jsonwebtoken::errors::Error> {
    let sub = player.id.to_string();
    let updated_at = player.updated_at.to_rfc3339();
    let claims = Claims {
        sub: &sub,
        name: player.slug.as_str(),
        given_name: player.name.as_str(),
        role: role_to_str(player.kind),
        avatar_url: player.avatar_url.as_str(),
        updated_at: &updated_at,
        iss: jwt.issuer.as_str(),
        aud: jwt.audience.as_str(),
        exp: (Utc::now() + jwt.expires).timestamp(),
    };
    encode(&Header::new(Algorithm::HS256), &claims, &jwt.encoding)
}

fn role_to_str(kind: PlayerType) -> &'static str {
    match kind {
        PlayerType::User => "User",
        PlayerType::Admin => "Admin",
    }
}

fn role_from_str(role: &str) -> PlayerType {
    match role {
        "Admin" => PlayerType::Admin,
        _ => PlayerType::User,
    }
}

/// The authenticated caller, extracted from a `Bearer` token. Endpoints that
/// require `[Authorize]` take this as an argument.
pub struct AuthPlayer {
    pub id: u64,
    pub slug: String,
    pub role: PlayerType,
}

impl AuthPlayer {
    pub fn is_admin(&self) -> bool {
        self.role == PlayerType::Admin
    }
}

impl FromRequestParts<AppState> for AuthPlayer {
    type Rejection = Response;

    async fn from_request_parts(
        parts: &mut Parts,
        state: &AppState,
    ) -> Result<Self, Self::Rejection> {
        let token = parts
            .headers
            .get(AUTHORIZATION)
            .and_then(|h| h.to_str().ok())
            .and_then(|h| h.strip_prefix("Bearer "))
            .ok_or_else(|| StatusCode::UNAUTHORIZED.into_response())?;

        let data = decode::<DecodedClaims>(token, &state.jwt.decoding, &state.jwt.validation)
            .map_err(|_| StatusCode::UNAUTHORIZED.into_response())?;

        let id = data
            .claims
            .sub
            .parse()
            .map_err(|_| StatusCode::UNAUTHORIZED.into_response())?;

        Ok(AuthPlayer {
            id,
            slug: data.claims.name,
            role: role_from_str(&data.claims.role),
        })
    }
}
