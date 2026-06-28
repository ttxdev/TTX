use serde::Deserialize;
use ttx::creators::model::{Creator, StreamStatus};
use ttx::data::Db;
use ttx::error::Result;
use ttx::platforms::Platform;
use ttx::players::PlayerType;
use ttx::players::model::Player;
use ttx::primitives::Meta;
use ttx::users::User;

const CREATORS_JSON: &str = include_str!("seed/creators.json");
const PLAYERS_JSON: &str = include_str!("seed/players.json");

#[derive(Debug, Deserialize)]
struct UserSeed {
    name: String,
    slug: String,
    platform_id: String,
    platform: u8,
    avatar_url: String,
}

impl UserSeed {
    fn to_user(&self) -> User {
        User {
            meta: Meta::default(),
            name: self.name.clone(),
            slug: self.slug.clone(),
            platform_id: self.platform_id.clone(),
            platform: platform_from(self.platform),
            avatar_url: self.avatar_url.clone(),
        }
    }
}

#[derive(Debug, Deserialize)]
struct CreatorSeed {
    #[serde(flatten)]
    user: UserSeed,
    ticker: String,
}

#[derive(Debug, Deserialize)]
struct PlayerSeed {
    #[serde(flatten)]
    user: UserSeed,
    #[serde(rename = "type")]
    kind: u8,
}

fn platform_from(value: u8) -> Platform {
    match value {
        0 => Platform::Twitch,
        _ => Platform::Twitch,
    }
}

fn player_type_from(value: u8) -> PlayerType {
    match value {
        1 => PlayerType::Admin,
        _ => PlayerType::User,
    }
}

pub async fn seed(db: &Db) -> Result<()> {
    if db.count_players(None).await? > 0 || db.count_creators(None, None).await? > 0 {
        tracing::info!("seed skipped, database already populated");
        return Ok(());
    }

    let players: Vec<PlayerSeed> =
        serde_json::from_str(PLAYERS_JSON).expect("embedded players.json is valid");
    let creators: Vec<CreatorSeed> =
        serde_json::from_str(CREATORS_JSON).expect("embedded creators.json is valid");

    for data in &players {
        let mut player = Player {
            user: data.user.to_user(),
            credits: 0.0,
            portfolio: 0.0,
            kind: player_type_from(data.kind),
            history: Vec::new(),
            transactions: Vec::new(),
            lootboxes: Vec::new(),
        };
        db.insert_player(&mut player).await?;
    }

    for data in &creators {
        let mut creator = Creator {
            user: data.user.to_user(),
            ticker: data.ticker.clone(),
            value: Creator::STARTER_VALUE,
            stream_status: StreamStatus::default(),
            transactions: Vec::new(),
            history: Vec::new(),
        };
        db.insert_creator(&mut creator).await?;
    }

    tracing::info!(
        "seeded {} players, {} creators",
        players.len(),
        creators.len()
    );
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn embedded_fixtures_parse() {
        let players: Vec<PlayerSeed> = serde_json::from_str(PLAYERS_JSON).unwrap();
        let creators: Vec<CreatorSeed> = serde_json::from_str(CREATORS_JSON).unwrap();

        assert!(!players.is_empty());
        assert!(!creators.is_empty());

        let player = &players[0];
        assert!(!player.user.slug.is_empty());
        assert_eq!(player_type_from(player.kind), PlayerType::Admin);
        assert_eq!(platform_from(player.user.platform), Platform::Twitch);

        let creator = &creators[0];
        assert!(!creator.ticker.is_empty());
        assert!(creator.user.platform_id.parse::<u64>().is_ok());
    }
}
