DROP TABLE IF EXISTS public."__EFMigrationsHistory";

ALTER TABLE IF EXISTS public.creators            RENAME CONSTRAINT "PK_creators"            TO creators_pkey;
ALTER TABLE IF EXISTS public.players             RENAME CONSTRAINT "PK_players"             TO players_pkey;
ALTER TABLE IF EXISTS public.transactions        RENAME CONSTRAINT "PK_transactions"        TO transactions_pkey;
ALTER TABLE IF EXISTS public.loot_boxes          RENAME CONSTRAINT "PK_loot_boxes"          TO loot_boxes_pkey;
ALTER TABLE IF EXISTS public.creator_applications RENAME CONSTRAINT "PK_creator_applications" TO creator_applications_pkey;
ALTER TABLE IF EXISTS public.creator_opt_outs    RENAME CONSTRAINT "PK_creator_opt_outs"    TO creator_opt_outs_pkey;

ALTER TABLE IF EXISTS public.transactions        RENAME CONSTRAINT "FK_transactions_creators_creator_id"          TO transactions_creator_id_fkey;
ALTER TABLE IF EXISTS public.transactions        RENAME CONSTRAINT "FK_transactions_players_player_id"            TO transactions_player_id_fkey;
ALTER TABLE IF EXISTS public.loot_boxes          RENAME CONSTRAINT "FK_loot_boxes_players_player_id"              TO loot_boxes_player_id_fkey;
ALTER TABLE IF EXISTS public.loot_boxes          RENAME CONSTRAINT "FK_loot_boxes_creators_result_id"             TO loot_boxes_result_id_fkey;
ALTER TABLE IF EXISTS public.votes               RENAME CONSTRAINT "FK_votes_creators_creator_id"                 TO votes_creator_id_fkey;
ALTER TABLE IF EXISTS public.player_portfolios   RENAME CONSTRAINT "FK_player_portfolios_players_player_id"        TO player_portfolios_player_id_fkey;
ALTER TABLE IF EXISTS public.creator_applications RENAME CONSTRAINT "FK_creator_applications_players_submitter_id" TO creator_applications_submitter_id_fkey;

ALTER INDEX IF EXISTS "IX_creators_slug"                   RENAME TO ix_creators_slug;
ALTER INDEX IF EXISTS "IX_creators_ticker"                 RENAME TO ix_creators_ticker;
ALTER INDEX IF EXISTS "IX_creators_platform_platform_id"   RENAME TO ix_creators_platform_platform_id;
ALTER INDEX IF EXISTS "IX_players_slug"                    RENAME TO ix_players_slug;
ALTER INDEX IF EXISTS "IX_players_platform_id"             RENAME TO ix_players_platform_id;
ALTER INDEX IF EXISTS "IX_players_type"                    RENAME TO ix_players_type;
ALTER INDEX IF EXISTS "IX_transactions_creator_id"         RENAME TO ix_transactions_creator_id;
ALTER INDEX IF EXISTS "IX_transactions_player_id"          RENAME TO ix_transactions_player_id;
ALTER INDEX IF EXISTS "IX_loot_boxes_player_id"            RENAME TO ix_loot_boxes_player_id;
ALTER INDEX IF EXISTS "IX_loot_boxes_result_id"            RENAME TO ix_loot_boxes_result_id;
ALTER INDEX IF EXISTS "IX_votes_creator_id_time"           RENAME TO ix_votes_creator_id_time;
ALTER INDEX IF EXISTS "IX_player_portfolios_player_id_time" RENAME TO ix_player_portfolios_player_id_time;
ALTER INDEX IF EXISTS "IX_creator_applications_submitter_id" RENAME TO ix_creator_applications_submitter_id;
ALTER INDEX IF EXISTS "IX_creator_opt_outs_platform_id"    RENAME TO ix_creator_opt_outs_platform_id;

ALTER TABLE IF EXISTS public.creator_applications RENAME COLUMN "Platform" TO platform;
ALTER TABLE IF EXISTS public.creator_opt_outs     RENAME COLUMN "Platform" TO platform;
ALTER TABLE IF EXISTS public.creator_opt_outs     RENAME COLUMN "Reason"   TO reason;

ALTER TABLE IF EXISTS public.creators ALTER COLUMN stream_started_at DROP DEFAULT;
ALTER TABLE IF EXISTS public.creators ALTER COLUMN stream_ended_at   DROP DEFAULT;
