use std::collections::HashMap;
use std::sync::Mutex;
use std::time::{Duration, Instant};

use crate::creators::Creator;
use crate::error::{Error, Result};
use crate::jobs::{StreamMonitorAdapter, StreamUpdateEvent};
use crate::platforms::twitch::TwitchOAuthOptions;
use crate::primitives::Id;
use async_trait::async_trait;
use chrono::{DateTime, Utc};
use reqwest::Client;
use serde::Deserialize;
use tokio::sync::mpsc::Sender;
use tokio_util::sync::CancellationToken;

const TOKEN_URL: &str = "https://id.twitch.tv/oauth2/token";
const STREAMS_URL: &str = "https://api.twitch.tv/helix/streams";
const MAX_PER_BATCH: usize = 100;
const CYCLE_MS: u64 = 5_000;
const MIN_DELAY_MS: u64 = 500;

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
struct StreamsResponse {
    data: Vec<StreamData>,
}

#[derive(Deserialize)]
struct StreamData {
    user_login: String,
    #[serde(default)]
    started_at: Option<String>,
}

#[derive(Default)]
struct State {
    watched: HashMap<String, Creator>,
    last_live: HashMap<Id, bool>,
}

pub struct TwitchStreamMonitorAdapter {
    http: Client,
    options: TwitchOAuthOptions,
    state: Mutex<State>,
    app_token: tokio::sync::Mutex<Option<(String, Instant)>>,
}

impl TwitchStreamMonitorAdapter {
    pub fn new(options: TwitchOAuthOptions) -> Self {
        Self {
            http: Client::new(),
            options,
            state: Mutex::new(State::default()),
            app_token: tokio::sync::Mutex::new(None),
        }
    }

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
        let ttl = Duration::from_secs(resp.expires_in.saturating_sub(60).max(60));
        *guard = Some((resp.access_token.clone(), Instant::now() + ttl));
        Ok(resp.access_token)
    }

    async fn live_streams(
        &self,
        token: &str,
        chunk: &[String],
    ) -> Result<HashMap<String, Option<DateTime<Utc>>>> {
        let query: Vec<(&str, &str)> = chunk.iter().map(|s| ("user_login", s.as_str())).collect();
        let resp: StreamsResponse = self
            .http
            .get(STREAMS_URL)
            .header("Client-Id", &self.options.client_id)
            .bearer_auth(token)
            .query(&query)
            .send()
            .await
            .map_err(ext)?
            .error_for_status()
            .map_err(ext)?
            .json()
            .await
            .map_err(ext)?;

        let mut live = HashMap::new();
        for stream in resp.data {
            let started = stream
                .started_at
                .as_deref()
                .and_then(|t| DateTime::parse_from_rfc3339(t).ok())
                .map(|dt| dt.with_timezone(&Utc));
            live.insert(stream.user_login.to_lowercase(), started);
        }
        Ok(live)
    }

    async fn check_streams(&self, sink: &Sender<StreamUpdateEvent>) -> Result<()> {
        let slugs: Vec<String> = {
            let state = self.state.lock().unwrap();
            if state.watched.is_empty() {
                return Ok(());
            }
            state.watched.keys().cloned().collect()
        };

        let token = self.app_token().await?;

        for chunk in slugs.chunks(MAX_PER_BATCH) {
            let live = self.live_streams(&token, chunk).await?;

            let mut events = Vec::new();
            {
                let mut state = self.state.lock().unwrap();
                for slug in chunk {
                    let creator_id = match state.watched.get(slug) {
                        Some(creator) => creator.id(),
                        None => continue,
                    };
                    let is_live_now = live.contains_key(slug);
                    let was_live = state.last_live.get(&creator_id).copied().unwrap_or(false);

                    if is_live_now && !was_live {
                        let at = live.get(slug).copied().flatten().unwrap_or_else(Utc::now);
                        events.push(StreamUpdateEvent {
                            creator_id,
                            is_live: true,
                            at,
                        });
                        state.last_live.insert(creator_id, true);
                    } else if !is_live_now && was_live {
                        events.push(StreamUpdateEvent {
                            creator_id,
                            is_live: false,
                            at: Utc::now(),
                        });
                        state.last_live.insert(creator_id, false);
                    }
                }
            }

            for event in events {
                if sink.send(event).await.is_err() {
                    return Ok(());
                }
            }
        }
        Ok(())
    }
}

#[async_trait]
impl StreamMonitorAdapter for TwitchStreamMonitorAdapter {
    async fn start(
        &self,
        sink: Sender<StreamUpdateEvent>,
        cancel: CancellationToken,
    ) -> Result<()> {
        tracing::info!("Starting Twitch stream monitor");
        while !cancel.is_cancelled() {
            let started = Instant::now();
            let count = self.state.lock().unwrap().watched.len();

            if let Err(err) = self.check_streams(&sink).await {
                tracing::error!(?err, "stream check failed");
            }

            let batches = count.div_ceil(MAX_PER_BATCH).max(1) as u64;
            let per_batch = CYCLE_MS / batches;
            let processing = started.elapsed().as_millis() as u64;
            let delay = per_batch.saturating_sub(processing).max(MIN_DELAY_MS);

            tokio::select! {
                _ = cancel.cancelled() => break,
                _ = tokio::time::sleep(Duration::from_millis(delay)) => {}
            }
        }
        Ok(())
    }

    fn set_creators(&self, creators: Vec<Creator>) {
        let mut state = self.state.lock().unwrap();
        state.watched.clear();
        for creator in creators {
            let id = creator.id();
            let slug = creator.user.slug.to_lowercase();
            state.last_live.entry(id).or_insert(false);
            state.watched.insert(slug, creator);
        }
    }

    fn remove_creator(&self, creator_id: Id) -> bool {
        let mut state = self.state.lock().unwrap();
        let slug = state
            .watched
            .iter()
            .find(|(_, creator)| creator.id() == creator_id)
            .map(|(slug, _)| slug.clone());
        match slug {
            Some(slug) => {
                state.watched.remove(&slug);
                state.last_live.remove(&creator_id);
                true
            }
            None => false,
        }
    }
}
