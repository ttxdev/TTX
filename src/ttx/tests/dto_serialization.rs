use serde_json::json;
use ttx::creators::Creator;
use ttx::creators::dto::CreatorPartialDto;
use ttx::creators::events::UpdateCreatorValueEvent;
use ttx::dto::pagination::PaginationDto;
use ttx::events::Event;
use ttx::platforms::{Platform, PlatformId, PlatformUser};
use ttx::players::Player;
use ttx::players::dto::PlayerPartialDto;

fn platform_user(id: PlatformId) -> PlatformUser {
    let username = format!("user{id}");
    let display_name = format!("User {id}");
    PlatformUser {
        id,
        username,
        display_name,
        avatar_url: "https://example.test/avatar.png".to_string(),
    }
}

#[test]
fn creator_partial_dto_json_shape() {
    let mut c = Creator::create(
        &platform_user(5.to_string()),
        "NAPPY".to_string(),
        Platform::Twitch,
    );
    c.user.meta.id = 5;
    c.value = 12.5;

    let v = serde_json::to_value(CreatorPartialDto::create(&c)).unwrap();

    assert_eq!(v["id"], json!(5));
    assert_eq!(v["slug"], json!("user5"));
    assert_eq!(v["ticker"], json!("NAPPY"));
    assert_eq!(v["value"], json!(12.5));
    assert_eq!(v["platform_id"], json!("5"));
    assert_eq!(v["platform_url"], json!("https://twitch.tv/user5"));
    assert!(v["stream_status"]["is_live"].is_boolean());
    assert!(v["history"].is_array());
}

#[test]
fn player_partial_dto_renames_kind_to_type() {
    let mut p = Player::create(&platform_user(7.to_string()), Some(100.0));
    p.user.meta.id = 7;

    let v = serde_json::to_value(PlayerPartialDto::create(&p)).unwrap();

    assert!(v.get("type").is_some());
    assert!(v.get("kind").is_none());
    assert_eq!(v["credits"], json!(100.0));
    assert_eq!(v["value"], json!(100.0));
}

#[test]
fn event_envelope_carries_type_tag() {
    let mut c = Creator::create(
        &platform_user(3.to_string()),
        "ABC".to_string(),
        Platform::Twitch,
    );
    c.user.meta.id = 3;
    let vote = c.apply_net_change(2.0);

    let v = UpdateCreatorValueEvent::create(&vote).to_value();

    assert_eq!(v["type"], json!("UpdateCreatorValueEvent"));
    assert_eq!(v["vote"]["creator_id"], json!(3));
    assert!(v["vote"]["value"].is_number());
}

#[test]
fn pagination_dto_shape() {
    let page = PaginationDto {
        data: vec![1_i64, 2, 3],
        total: 42,
    };
    let v = serde_json::to_value(page).unwrap();
    assert_eq!(v["data"], json!([1, 2, 3]));
    assert_eq!(v["total"], json!(42));
}
