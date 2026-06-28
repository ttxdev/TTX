use std::time::{Duration, Instant};

use crate::error::{Error, Result};
use crate::platforms::{PlatformId, PlatformUser, PlatformUserService};
use async_trait::async_trait;
use reqwest::Client;
use serde::Deserialize;
use tokio::sync::Mutex;

const TOKEN_URL: &str = "https://id.twitch.tv/oauth2/token";
const USERS_URL: &str = "https://api.twitch.tv/helix/users";

#[derive(Debug, Clone, Deserialize)]
pub struct TwitchOAuthOptions {
    pub client_id: String,
    pub client_secret: String,
    pub redirect_uri: String,
}

fn ext(e: impl std::fmt::Display) -> Error {
    Error::External(e.to_string())
}

#[derive(Deserialize)]
struct TokenResponse {
    access_token: String,
    #[serde(default)]
    expires_in: u64,
}

#[derive(Deserialize)]
struct UsersResponse {
    data: Vec<TwitchUser>,
}

#[derive(Deserialize)]
struct TwitchUser {
    id: String,
    login: String,
    display_name: String,
    profile_image_url: String,
}

fn convert(user: TwitchUser) -> PlatformUser {
    PlatformUser {
        id: user.id,
        username: user.login,
        display_name: user.display_name,
        avatar_url: user.profile_image_url,
    }
}

pub struct TwitchUserService {
    http: Client,
    options: TwitchOAuthOptions,
    app_token: Mutex<Option<(String, Instant)>>,
}

impl TwitchUserService {
    pub fn new(options: TwitchOAuthOptions) -> Self {
        Self {
            http: Client::new(),
            options,
            app_token: Mutex::new(None),
        }
    }

    /// A cached client-credentials token for Helix calls.
    async fn app_token(&self) -> Result<String> {
        let mut guard = self.app_token.lock().await;
        if let Some((token, expiry)) = guard.as_ref()
            && *expiry > Instant::now()
        {
            return Ok(token.clone());
        }

        let resp: TokenResponse = self
            .http
            .post(TOKEN_URL)
            .query(&[
                ("client_id", self.options.client_id.as_str()),
                ("client_secret", self.options.client_secret.as_str()),
                ("grant_type", "client_credentials"),
            ])
            .send()
            .await
            .map_err(ext)?
            .error_for_status()
            .map_err(ext)?
            .json()
            .await
            .map_err(ext)?;

        // Renew a minute early; never less than a minute out.
        let ttl = Duration::from_secs(resp.expires_in.saturating_sub(60).max(60));
        *guard = Some((resp.access_token.clone(), Instant::now() + ttl));
        Ok(resp.access_token)
    }

    async fn fetch_user(
        &self,
        token: &str,
        key: &str,
        value: &str,
    ) -> Result<Option<PlatformUser>> {
        let resp: UsersResponse = self
            .http
            .get(USERS_URL)
            .header("Client-Id", &self.options.client_id)
            .bearer_auth(token)
            .query(&[(key, value)])
            .send()
            .await
            .map_err(ext)?
            .error_for_status()
            .map_err(ext)?
            .json()
            .await
            .map_err(ext)?;
        Ok(resp.data.into_iter().next().map(convert))
    }
}

#[async_trait]
impl PlatformUserService for TwitchUserService {
    async fn get_user_by_username(&self, username: &str) -> Result<Option<PlatformUser>> {
        let token = self.app_token().await?;
        self.fetch_user(&token, "login", username).await
    }

    async fn get_user_by_id(&self, id: PlatformId) -> Result<Option<PlatformUser>> {
        let token = self.app_token().await?;
        self.fetch_user(&token, "id", &id.to_string()).await
    }

    async fn resolve_by_oauth(&self, code: &str) -> Result<Option<PlatformUser>> {
        // Exchange the auth code for the user's own access token...
        let token: TokenResponse = self
            .http
            .post(TOKEN_URL)
            .query(&[
                ("client_id", self.options.client_id.as_str()),
                ("client_secret", self.options.client_secret.as_str()),
                ("code", code),
                ("grant_type", "authorization_code"),
                ("redirect_uri", self.options.redirect_uri.as_str()),
            ])
            .send()
            .await
            .map_err(ext)?
            .error_for_status()
            .map_err(ext)?
            .json()
            .await
            .map_err(ext)?;

        // ...then GetUsers with no filter returns the authenticated user.
        let resp: UsersResponse = self
            .http
            .get(USERS_URL)
            .header("Client-Id", &self.options.client_id)
            .bearer_auth(&token.access_token)
            .send()
            .await
            .map_err(ext)?
            .error_for_status()
            .map_err(ext)?
            .json()
            .await
            .map_err(ext)?;
        Ok(resp.data.into_iter().next().map(convert))
    }

    fn get_login_url(&self) -> String {
        format!(
            "https://id.twitch.tv/oauth2/authorize?client_id={}&redirect_uri={}&response_type=code&scope=",
            self.options.client_id, self.options.redirect_uri
        )
    }
}
