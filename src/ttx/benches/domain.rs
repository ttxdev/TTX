//! Micro-benchmarks for the CPU-bound hot paths that run on every request:
//! share aggregation, rarity computation, portfolio snapshots, and DTO build +
//! serialization (the per-item cost of the creator/player index endpoints).
//!
//! Run with `cargo bench -p ttx`. These measure in-memory work only; DB/cache
//! throughput needs the gated integration tests / a load test.

use std::collections::HashMap;
use std::hint::black_box;

use criterion::{BatchSize, BenchmarkId, Criterion, criterion_group, criterion_main};
use ttx::creators::Creator;
use ttx::creators::dto::CreatorDto;
use ttx::lootboxes::CreatorRarity;
use ttx::platforms::{Platform, PlatformId, PlatformUser};
use ttx::players::Player;
use ttx::primitives::Id;
use ttx::transactions::Transaction;

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

fn creator(id: Id, value: f64) -> Creator {
    let mut c = Creator::create(
        &platform_user(id.to_string()),
        format!("TICK{id}"),
        Platform::Twitch,
    );
    c.user.meta.id = id;
    c.value = value;
    c
}

fn player(id: Id) -> Player {
    let mut p = Player::create(&platform_user(id.to_string()), Some(1_000_000.0));
    p.user.meta.id = id;
    p
}

/// A creator carrying `n` buy transactions from `n` distinct players (worst case
/// for the per-player grouping in `get_shares`).
fn creator_with_txs(n: u64) -> (Creator, HashMap<u64, Player>) {
    let mut c = creator(1, 10.0);
    let mut players = HashMap::new();
    let mut txs = Vec::with_capacity(n as usize);
    for pid in 0..n {
        let p = player(1000 + pid);
        txs.push(Transaction::create_buy(&p, &c, (pid % 5) + 1));
        players.insert(p.id(), p);
    }
    c.transactions = txs;
    (c, players)
}

fn bench_get_shares(c: &mut Criterion) {
    let mut group = c.benchmark_group("creator_get_shares");
    for n in [10u64, 100, 1000] {
        let (creator, _) = creator_with_txs(n);
        group.bench_with_input(BenchmarkId::from_parameter(n), &creator, |b, creator| {
            b.iter(|| black_box(creator.get_shares()))
        });
    }
    group.finish();
}

fn bench_take_portfolio_snapshot(c: &mut Criterion) {
    let mut p = player(1);
    let cr = creator(2, 10.0);
    for _ in 0..200 {
        p.transactions
            .push(Transaction::create_buy(&p.clone(), &cr, 1));
    }
    c.bench_function("take_portfolio_snapshot/200", |b| {
        b.iter_batched(
            || p.clone(),
            |mut p| black_box(p.take_portfolio_snapshot(|_| 10.0)),
            BatchSize::SmallInput,
        )
    });
}

fn bench_creator_rarity(c: &mut Criterion) {
    let cr = creator(1, 12.5);
    c.bench_function("creator_rarity_create", |b| {
        b.iter_batched(
            || cr.clone(),
            |cr| black_box(CreatorRarity::create(black_box(100.0), cr)),
            BatchSize::SmallInput,
        )
    });
}

fn bench_creator_dto_build_and_serialize(c: &mut Criterion) {
    let mut group = c.benchmark_group("creator_dto_create+serialize");
    for n in [10u64, 100] {
        let (creator, players) = creator_with_txs(n);
        group.bench_with_input(BenchmarkId::from_parameter(n), &n, |b, _| {
            b.iter(|| {
                let dto = CreatorDto::create(black_box(&creator), black_box(&players));
                black_box(serde_json::to_value(&dto).unwrap())
            })
        });
    }
    group.finish();
}

criterion_group!(
    benches,
    bench_get_shares,
    bench_take_portfolio_snapshot,
    bench_creator_rarity,
    bench_creator_dto_build_and_serialize,
);
criterion_main!(benches);
