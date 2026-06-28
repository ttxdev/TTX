use ttx::creators::Creator;
use ttx::error::Error;
use ttx::lootboxes::LootBox;
use ttx::lootboxes::{CreatorRarity, Rarity};
use ttx::platforms::{Platform, PlatformId, PlatformUser};
use ttx::players::Player;
use ttx::players::model::{MAX_SHARES, STARTER_CREDITS};
use ttx::primitives::Meta;
use ttx::shares::Share;
use ttx::transactions::{Transaction, TransactionAction};
use ttx::users::User;

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

fn creator(id: u64, value: f64) -> Creator {
    let mut c = Creator::create(
        &platform_user(id.to_string()),
        format!("TICK{id}"),
        Platform::Twitch,
    );
    c.user.meta.id = id;
    c.value = value;
    c
}

fn player(id: u64, credits: f64) -> Player {
    let mut p = Player::create(&platform_user(id.to_string()), Some(credits));
    p.user.meta.id = id;
    p
}

fn shares_of(player: &Player, creator_id: u64) -> u64 {
    player
        .get_shares()
        .into_iter()
        .find(|s| s.creator_id == creator_id)
        .map_or(0, |s| s.quantity)
}

#[test]
fn player_create_defaults() {
    let p = Player::create(&platform_user(1.to_string()), None);
    assert_eq!(p.credits, STARTER_CREDITS);
    assert_eq!(p.portfolio, 0.0);
    assert_eq!(p.lootboxes.len(), 1, "starter lootbox is granted");
    assert!(p.transactions.is_empty());
}

#[test]
fn buy_deducts_credits_and_records_shares() {
    let mut p = player(1, 100.0);
    let c = creator(2, 10.0);

    let tx = p.buy(&c, 3).expect("buy succeeds");
    assert_eq!(tx.action, TransactionAction::Buy);
    assert_eq!(p.credits, 70.0);
    assert_eq!(shares_of(&p, c.id()), 3);
}

#[test]
fn buy_rejects_insufficient_funds() {
    let mut p = player(1, 5.0);
    let c = creator(2, 10.0);
    assert!(matches!(p.buy(&c, 1), Err(Error::InvalidAction(_))));
    assert_eq!(p.credits, 5.0, "credits unchanged on failure");
}

#[test]
fn buy_rejects_exceeding_max_shares() {
    let mut p = player(1, 1_000_000.0);
    let c = creator(2, 0.001);
    assert!(matches!(
        p.buy(&c, MAX_SHARES + 1),
        Err(Error::InvalidAction(_))
    ));
}

#[test]
fn sell_reduces_shares_and_adds_credits() {
    let mut p = player(1, 100.0);
    let c = creator(2, 10.0);

    p.buy(&c, 5).expect("buy");
    let tx = p.sell(&c, 2).expect("sell");
    assert_eq!(tx.action, TransactionAction::Sell);
    assert_eq!(shares_of(&p, c.id()), 3);
    assert_eq!(p.credits, 100.0 - 50.0 + 20.0);
}

#[test]
fn sell_rejects_insufficient_shares() {
    let mut p = player(1, 100.0);
    let c = creator(2, 10.0);
    p.buy(&c, 1).expect("buy");
    assert!(matches!(p.sell(&c, 5), Err(Error::InvalidAction(_))));
}

#[test]
fn give_grants_one_free_share() {
    let mut p = player(1, 100.0);
    let c = creator(2, 10.0);

    let tx = p.give(&c);
    assert_eq!(tx.action, TransactionAction::Open);
    assert!(tx.is_gain());
    assert_eq!(shares_of(&p, c.id()), 1);
    assert_eq!(p.credits, 100.0, "Open grants are free");
}

#[test]
fn take_portfolio_snapshot_values_held_shares() {
    let mut p = player(1, 100.0);
    let c = creator(2, 10.0);
    p.buy(&c, 3).expect("buy");

    let snapshot = p.take_portfolio_snapshot(|id| if id == c.id() { 12.0 } else { 0.0 });
    assert_eq!(snapshot.player_id, p.id());
    assert_eq!(snapshot.value, 36.0); // 3 shares * 12.0
    assert_eq!(p.portfolio, 36.0);
}

#[test]
fn apply_net_change_clamps_to_min_value() {
    let mut c = creator(1, 1.0);
    let vote = c.apply_net_change(-5.0);
    assert_eq!(c.value, Creator::MIN_VALUE);
    assert_eq!(vote.value, Creator::MIN_VALUE);
    assert_eq!(vote.creator_id, c.id());
}

#[test]
fn apply_net_change_increases_value() {
    let mut c = creator(1, 10.0);
    c.apply_net_change(5.0);
    assert_eq!(c.value, 15.0);
}

#[test]
fn creator_get_shares_groups_by_player() {
    let mut c = creator(1, 10.0);
    let a = player(10, 100.0);
    let b = player(20, 100.0);
    c.transactions = vec![
        Transaction::create_buy(&a, &c, 2),
        Transaction::create_buy(&b, &c, 3),
        Transaction::create_sell(&a, &c, 1),
    ];

    let mut shares = c.get_shares();
    shares.sort_by_key(|s| s.player_id);
    assert_eq!(shares.len(), 2);
    assert_eq!(shares[0].quantity, 1); // player 10: 2 - 1
    assert_eq!(shares[1].quantity, 3); // player 20: 3
}

#[test]
fn transaction_total_and_is_gain() {
    let p = player(1, 100.0);
    let c = creator(2, 10.0);

    let buy = Transaction::create_buy(&p, &c, 4);
    assert_eq!(buy.total(), 40.0);
    assert!(buy.is_gain());

    let sell = Transaction::create_sell(&p, &c, 4);
    assert!(!sell.is_gain());
    assert!(Transaction::create_open(&p, &c, 1).is_gain());
}

#[test]
fn share_count_accumulates_gains_and_losses() {
    let p = player(1, 100.0);
    let c = creator(2, 10.0);
    let mut share = Share::new(c.id(), p.id());

    share.count(&Transaction::create_buy(&p, &c, 5));
    share.count(&Transaction::create_sell(&p, &c, 2));
    assert_eq!(share.quantity, 3);
}

#[test]
fn creator_rarity_boundaries() {
    // calc = value / sum * 100. Buckets: [0,1) [1,5) [5,20) [20,..].
    assert_eq!(
        CreatorRarity::create(100.0, creator(1, 0.5)).rarity,
        Rarity::Pennies
    );
    assert_eq!(
        CreatorRarity::create(100.0, creator(2, 3.0)).rarity,
        Rarity::Common
    );
    assert_eq!(
        CreatorRarity::create(100.0, creator(3, 10.0)).rarity,
        Rarity::Rare
    );
    assert_eq!(
        CreatorRarity::create(100.0, creator(4, 50.0)).rarity,
        Rarity::Epic
    );
}

#[test]
fn lootbox_can_only_be_opened_once() {
    let p = player(1, 100.0);
    let c = creator(2, 10.0);
    let mut box_ = LootBox::create(&p);

    assert!(!box_.is_open());
    box_.set_result(&c).expect("first open");
    assert!(box_.is_open());
    assert!(matches!(box_.set_result(&c), Err(Error::InvalidAction(_))));
}

#[test]
fn user_sync_reports_and_applies_changes() {
    let mut user = User {
        meta: Meta::default(),
        name: "Old".to_string(),
        slug: "old".to_string(),
        platform_id: "1".to_string(),
        platform: Platform::Twitch,
        avatar_url: "old.png".to_string(),
    };
    let updated = PlatformUser {
        id: "1".to_string(),
        username: "new".to_string(),
        display_name: "New".to_string(),
        avatar_url: "new.png".to_string(),
    };

    assert!(user.sync(&updated), "changed fields => true");
    assert_eq!(user.name, "New");
    assert_eq!(user.slug, "new");
    assert_eq!(user.avatar_url, "new.png");
    assert!(!user.sync(&updated), "no change => false");
}
