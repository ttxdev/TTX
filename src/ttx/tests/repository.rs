use std::time::{SystemTime, UNIX_EPOCH};

use sqlx::postgres::PgPoolOptions;
use ttx::creators::Creator;
use ttx::data::Db;
use ttx::platforms::{Platform, PlatformUser};
use ttx::players::Player;

/// Returns a connected [`Db`], or `None` (logging a skip) when the env var is
/// absent.
async fn db() -> Option<Db> {
    let url = std::env::var("TEST_DATABASE_URL").ok()?;
    let pool = PgPoolOptions::new()
        .max_connections(2)
        .connect(&url)
        .await
        .expect("connect to TEST_DATABASE_URL");
    Some(Db::new(pool))
}

/// A process-unique suffix so tests don't collide with existing rows or each
/// other (slug / ticker / platform_id are all unique constraints).
fn unique() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap()
        .as_nanos() as u64
        % 1_000_000_000
}

#[tokio::test]
async fn creator_crud_roundtrip() {
    let Some(db) = db().await else {
        eprintln!("skipping: TEST_DATABASE_URL not set");
        return;
    };

    let n = unique();
    let slug = format!("creator_slug_{n}");
    let ticker = format!("TT{n}");
    let user = PlatformUser {
        id: n.to_string(),
        username: slug.clone(),
        display_name: format!("Creator {n}"),
        avatar_url: "https://example.test/a.png".to_string(),
    };

    let mut creator = Creator::create(&user, ticker.clone(), Platform::Twitch);
    db.insert_creator(&mut creator).await.expect("insert");
    assert_ne!(creator.id(), 0, "generated id is populated");

    let found = db
        .creator_by_slug(&slug)
        .await
        .expect("query")
        .expect("creator exists");
    assert_eq!(found.ticker, ticker);
    assert_eq!(found.id(), creator.id());
    assert!(db.ticker_exists(&ticker).await.expect("ticker_exists"));

    db.delete_creator(creator.id()).await.expect("delete");
    assert!(
        db.creator_by_slug(&slug).await.expect("query").is_none(),
        "creator removed"
    );
}

#[tokio::test]
async fn player_insert_and_lookup() {
    let Some(db) = db().await else {
        eprintln!("skipping: TEST_DATABASE_URL not set");
        return;
    };

    let n = unique();
    let slug = format!("player_slug_{n}");
    let user = PlatformUser {
        id: n.to_string(),
        username: slug.clone(),
        display_name: format!("Player {n}"),
        avatar_url: "https://example.test/a.png".to_string(),
    };

    let mut p = Player::create(&user, Some(100.0));
    db.insert_player(&mut p).await.expect("insert");
    assert_ne!(p.id(), 0);

    let found = db
        .player_by_slug(&slug)
        .await
        .expect("query")
        .expect("player exists");
    assert_eq!(found.id(), p.id());
    assert_eq!(found.credits, 100.0);

    // Player::create grants a starter lootbox, which insert_player persists.
    let boxes = db.player_lootboxes(p.id(), false).await.expect("lootboxes");
    assert_eq!(boxes.len(), 1);
}
