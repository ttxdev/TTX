//! Websocket hubs. Replace the SignalR `EventHub`/`VoteHub`/`PortfolioHub`.
//!
//! Each socket subscribes to the shared event broadcaster and streams the
//! matching `{type, ...}` envelopes as JSON text. Inbound client messages
//! (the C# `SetCreator`/`SetPlayer` group joins) are not modelled; each hub
//! instead filters by event type.

use std::sync::Arc;

use axum::extract::State;
use axum::extract::ws::{Message, WebSocket, WebSocketUpgrade};
use axum::response::Response;
use serde_json::Value;
use tokio::sync::broadcast::{Receiver, error::RecvError};

use crate::state::AppState;

pub async fn events(ws: WebSocketUpgrade, State(state): State<AppState>) -> Response {
    ws.on_upgrade(move |socket| forward(socket, state.events.subscribe_raw(), None))
}

pub async fn votes(ws: WebSocketUpgrade, State(state): State<AppState>) -> Response {
    const TYPES: &[&str] = &["UpdateCreatorValueEvent", "UpdatePlayerPortfolioEvent"];
    ws.on_upgrade(move |socket| forward(socket, state.events.subscribe_raw(), Some(TYPES)))
}

pub async fn portfolios(ws: WebSocketUpgrade, State(state): State<AppState>) -> Response {
    const TYPES: &[&str] = &["UpdatePlayerPortfolioEvent"];
    ws.on_upgrade(move |socket| forward(socket, state.events.subscribe_raw(), Some(TYPES)))
}

async fn forward(
    mut socket: WebSocket,
    mut rx: Receiver<Arc<Value>>,
    allowed: Option<&'static [&str]>,
) {
    loop {
        match rx.recv().await {
            Ok(value) => {
                let passes = allowed.is_none_or(|types| {
                    value
                        .get("type")
                        .and_then(|t| t.as_str())
                        .is_some_and(|t| types.contains(&t))
                });
                if passes
                    && socket
                        .send(Message::Text(value.to_string().into()))
                        .await
                        .is_err()
                {
                    break;
                }
            }
            Err(RecvError::Lagged(_)) => continue,
            Err(RecvError::Closed) => break,
        }
    }
}
