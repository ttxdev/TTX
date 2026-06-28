-- Performance indexes for the hot read paths (the paginated index/show
-- endpoints and the lootbox/live-creator queries). Each index notes the query
-- in the data layer it supports. Run as a plain (transactional) migration, so
-- no CREATE INDEX CONCURRENTLY here.

-- --- Name search (ILIKE '%term%') -------------------------------------------
-- `count_creators` / `index_creators` / `count_players` / `index_players` filter
-- with `name ILIKE '%search%'`. A leading wildcard cannot use a btree, so a
-- trigram GIN index is required to avoid a full table scan on search.
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE INDEX IF NOT EXISTS ix_creators_name_trgm ON creators USING gin (name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS ix_players_name_trgm ON players USING gin (name gin_trgm_ops);

-- --- Creator listing: filter + ordering -------------------------------------
-- `index_creators` orders by name / value and range-filters on `value >= $1`.
CREATE INDEX IF NOT EXISTS ix_creators_name ON creators (name);
CREATE INDEX IF NOT EXISTS ix_creators_value ON creators (value);

-- `live_creators` (stream monitor / digest) and `live_creators_with_min_value`
-- (lootbox rarities) filter on `stream_is_live = true` (a small, hot subset).
-- A partial index keeps it tiny and also serves the `value >= avg` range.
CREATE INDEX IF NOT EXISTS ix_creators_live_value ON creators (value) WHERE stream_is_live;

-- --- Player listing: ordering -----------------------------------------------
-- `index_players` orders by name / credits / portfolio.
CREATE INDEX IF NOT EXISTS ix_players_name ON players (name);
CREATE INDEX IF NOT EXISTS ix_players_credits ON players (credits);
CREATE INDEX IF NOT EXISTS ix_players_portfolio ON players (portfolio);

-- --- Transaction history: filter by owner, order by created_at --------------
-- `creator_transactions` / `player_transactions` do
-- `WHERE <owner>_id = $1 ORDER BY created_at`. The composite serves both the
-- lookup and the sort (and still backs the FK cascade), so the single-column
-- indexes from 0001 become redundant.
CREATE INDEX IF NOT EXISTS ix_transactions_creator_created ON transactions (creator_id, created_at);
CREATE INDEX IF NOT EXISTS ix_transactions_player_created ON transactions (player_id, created_at);
DROP INDEX IF EXISTS ix_transactions_creator_id;
DROP INDEX IF EXISTS ix_transactions_player_id;

-- --- Unopened lootboxes -----------------------------------------------------
-- `player_lootboxes(player_id, unopened_only = true)` (player Find + gamba)
-- filters `player_id = $1 AND result_id IS NULL`.
CREATE INDEX IF NOT EXISTS ix_loot_boxes_player_unopened
    ON loot_boxes (player_id) WHERE result_id IS NULL;
