use super::{ChatMonitorAdapter, Message};
use crate::error::{Error, Result};
use async_trait::async_trait;
use tokio::sync::Mutex;
use tokio::sync::mpsc::Sender;
use tokio_util::sync::CancellationToken;
use twitch_irc::login::StaticLoginCredentials;
use twitch_irc::message::ServerMessage;
use twitch_irc::{ClientConfig, SecureTCPTransport, TwitchIRCClient};

type IrcClient = TwitchIRCClient<SecureTCPTransport, StaticLoginCredentials>;

fn ext(e: impl std::fmt::Display) -> Error {
    Error::External(e.to_string())
}

pub struct TwitchChatAdapter {
    client: Mutex<Option<IrcClient>>,
}

impl TwitchChatAdapter {
    pub fn new() -> Self {
        Self {
            client: Mutex::new(None),
        }
    }
}

impl Default for TwitchChatAdapter {
    fn default() -> Self {
        Self::new()
    }
}

#[async_trait]
impl ChatMonitorAdapter for TwitchChatAdapter {
    async fn start(
        &self,
        channels: Vec<String>,
        sink: Sender<Message>,
        cancel: CancellationToken,
    ) -> Result<()> {
        let config = ClientConfig::default();
        let (mut incoming, client) = IrcClient::new(config);

        tokio::spawn(async move {
            loop {
                tokio::select! {
                    _ = cancel.cancelled() => break,
                    message = incoming.recv() => match message {
                        Some(ServerMessage::Privmsg(msg)) => {
                            let forwarded = Message {
                                slug: msg.channel_login,
                                content: msg.message_text,
                            };
                            if sink.send(forwarded).await.is_err() {
                                break;
                            }
                        }
                        Some(_) => {}
                        None => break,
                    }
                }
            }
        });

        for channel in channels {
            tracing::info!(channel = %channel, "joining");
            client.join(channel).map_err(ext)?;
        }

        *self.client.lock().await = Some(client);
        Ok(())
    }

    async fn add(&self, channel: String) -> Result<bool> {
        if let Some(client) = self.client.lock().await.as_ref() {
            tracing::info!("joining {}", channel);
            client.join(channel).map_err(ext)?;
            return Ok(true);
        }
        Ok(false)
    }

    async fn remove(&self, channel: String) -> Result<bool> {
        if let Some(client) = self.client.lock().await.as_ref() {
            tracing::info!("leaving {}", channel);
            client.part(channel);
            return Ok(true);
        }
        Ok(false)
    }
}
