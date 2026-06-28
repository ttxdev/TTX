# Caching plan

Companion to migration `0002_performance_indexes.sql`. The indexes cut per-query
cost; caching cuts *how often* the expensive queries run. This documents what to
cache, how, and how to keep it fresh.

**Status:** the **L2 series/aggregate cache** (§2) is implemented behind a
swappable `Cache` trait (`ttx::cache`): `get_creator_history` /
`get_player_history` / `average_creator_value` are cached at the `Db` boundary.
The backend is in-process `InMemoryCache` (moka) by default, or shared
`infra::RedisCache` when `REDIS_URL` is set — selected once via
`infra::cache_from_env()` and injected into `Db`. The L1 response cache (§2) and
event-driven invalidation (§5) remain future work; TTLs are the current
freshness mechanism.

## 1. Cost analysis — where the time goes

| Route | Handler | DB work | Cost |
|------|---------|---------|------|
| `GET /creators` | `routes::creators::index` | `count_creators` + `index_creators` + **`get_creator_history` (one TimescaleDB `time_bucket_gapfill`+`locf` query *per creator*)** | **Highest** — a page of 20 ⇒ ~22 queries, 20 of them windowed aggregates over `votes` |
| `GET /players` | `routes::players::index` | `count_players` + `index_players` + `get_player_history` (single `player_id = ANY(...)` gapfill) | High — 1 timeseries query but still a gapfill over `player_portfolios` |
| `GET /players/{username}`, `GET /players/me` | `players::show` / `get_me` | `player_by_slug` + `player_transactions` + `player_lootboxes` + `creators_by_ids` + `get_player_history` | Medium — several round-trips |
| `GET /creators/{slug}` | `creators::show` | `creator_by_slug` + `creator_transactions` + `players_by_ids` + `get_creator_history` (1) | Medium |
| lootbox open | `lootboxes::open_loot_box` | `average_creator_value` + `live_creators_with_min_value` (`get_creator_rarities`) | Medium, per gamba |

**Write cadence** (this drives TTLs): votes land every ~15s (creator-value job),
portfolio snapshots every ~60s (portfolio job), transactions on demand, stream
status on stream events. So the read data is only stale for at most one of those
cycles — short TTLs are safe.

The dominant cost is the **gap-filled history series** (`votes` /
`player_portfolios`), which is read on every list/detail view but only *changes*
once per job cycle. That makes it the prime cache target.

## 2. Two cache layers

- **L1 — response cache.** Cache the serialized `PaginationDto<…>` / `CreatorDto`
  / `PlayerDto` JSON keyed by the normalized request. Best for repeated identical
  GETs (home page, popular creators). Coarse, simplest.
- **L2 — series/aggregate cache.** Cache the results of `get_creator_history` /
  `get_player_history` (per entity), `average_creator_value`, and
  `get_creator_rarities` at the repository boundary. Reused across both the list
  and detail views and across pages, and it directly amortizes the N-query
  history cost.

Recommended order: **L2 first** (highest leverage, narrow blast radius), then L1
for `/creators` and `/players` if list latency is still a concern.

## 3. Key design

```
creators:index:{page}:{limit}:{search|-}:{orderBy|-}:{orderDir|-}      # L1
players:index:{page}:{limit}:{search|-}:{orderBy|-}:{orderDir|-}       # L1
creator:show:{id}:{step}:{before_secs}                                # L1 (key by id, not slug)
player:show:{id}:{step}:{before_secs}                                 # L1
hist:creator:{id}:{step}:{before_secs}:{bucket_epoch}                 # L2
hist:player:{id}:{step}:{before_secs}:{bucket_epoch}                  # L2
creators:avg_value                                                    # L2
lootbox:rarities                                                      # L2
```

- Prefix every key with a schema version (e.g. `v1:`) so deploys can invalidate
  en masse by bumping it.
- `bucket_epoch` = `now` truncated to the step. It makes the history key roll
  over exactly when a new bucket can appear, so the cache self-expires on the
  natural boundary in addition to its TTL.
- Key show/detail entries by **id**, not slug, so event-driven invalidation
  (which carries ids) is direct. Resolve slug→id once (cache that mapping too, or
  accept the cheap indexed `*_by_slug` lookup before the cache check).

## 4. TTLs

| Entry | TTL | Why |
|-------|-----|-----|
| index responses | 10s | < one vote cycle (15s); absorbs request bursts |
| show responses | 10s | same |
| history series | 15s | gains at most one point per job cycle; bucket key also rolls |
| `creators:avg_value` | 30–60s | moves slowly |
| `lootbox:rarities` | 15–30s | tracks live set + average |

## 5. Invalidation — event-driven, TTL as backstop

We already fan out domain events (Postgres `NOTIFY` → `EventBroadcaster`). The
API's listener task (`main.rs`, the `pg_events::listen` loop) is the natural place
to **also invalidate cache entries**:

| Event | Invalidate |
|-------|-----------|
| `UpdateCreatorValueEvent{creator_id}` | `creator:show:{id}:*`, `hist:creator:{id}:*`, `creators:avg_value` |
| `UpdatePlayerPortfolioEvent{player_id}` | `player:show:{id}:*`, `hist:player:{id}:*` |
| `CreateTransactionEvent{creator,player}` | `creator:show:{cid}:*`, `player:show:{pid}:*` |
| `UpdateStreamStatusEvent{creator_id}` | `lootbox:rarities`, `creator:show:{id}:*` |
| `CreateCreatorEvent` / opt-out | `creators:avg_value` (lists ride their 10s TTL) |

**List pages are deliberately not event-invalidated** (you can't cheaply know
which pages changed); their short TTL covers it. Only per-entity detail/series
entries get precise invalidation. TTL remains the safety net so correctness never
depends on perfect invalidation.

## 6. Implementation options

- **Phase 1 — in-process, no new infra (done).** `InMemoryCache` (`ttx::cache`)
  wraps a `moka::future::Cache` with per-entry TTL, behind the `Cache` trait so the
  backend is swappable. Zero ops; per-instance only. This is the default fallback.
- **Phase 2 — Redis (done; for >1 replica / shared cache).** `infra::RedisCache`
  implements the same `Cache` trait over a `redis` `ConnectionManager` (`SET … EX`
  for native per-key TTL). Selected when `REDIS_URL` is set; this is what the
  original C# stack used (Redis SignalR backplane). Cross-instance *invalidation*
  off the event channel is still future work — entries currently ride their TTL.

  Keys are namespaced `ttx:cache:hist:{creator|player}:{id}:{step}:{secs}` and
  `ttx:cache:avg:creator_value`. The `{creator|player}` segment matters now that a
  single backend holds both series (creator and player ids both start at 1).

Auth-gated mutations (`POST /transactions`, gamba, onboard, opt-out) and session
endpoints are **never** cached. `/players/me` is per-user — cache only keyed by the
authenticated id with a tiny TTL, or skip it.

## 7. Higher-leverage fixes to pair with caching

1. **Batch `get_creator_history` (N+1).** It loops one gapfill query per creator;
   even cached, a cold `/creators` page is ~20 round-trips. Rewrite to a single
   `creator_id = ANY($1)` query like `get_player_history`. The per-creator end time
   (`live ? now : ended_at`) can be supplied via a `LATERAL` join over a
   `(id, end_time)` VALUES list, or by bucketing to `now` for all and trimming
   ended streams in code. Biggest single win for the hottest route.
2. **TimescaleDB continuous aggregate** for the bucketed series, so the gapfill
   reads a rollup instead of raw `votes` / `player_portfolios`.
3. **HTTP caching.** Set `Cache-Control: public, max-age=10` (and/or `ETag`) on the
   public GET responses so browsers and any CDN/edge cache too — free tier-0 cache
   above the app.

## 8. Suggested first PR

L2 `moka` cache for `get_creator_history` / `get_player_history` /
`average_creator_value` + the N+1 batch rewrite (#7.1). That removes the worst
cost on the two hottest routes with minimal surface area and no new dependency.
