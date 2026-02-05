using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TTX.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "creator_opt_outs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    platform_id = table.Column<string>(type: "text", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creator_opt_outs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "creators",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ticker = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<long>(type: "bigint", nullable: false),
                    stream_is_live = table.Column<bool>(type: "boolean", nullable: false),
                    stream_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    stream_ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    platform_id = table.Column<string>(type: "text", nullable: false),
                    platform = table.Column<string>(type: "text", nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creators", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    credits = table.Column<long>(type: "bigint", nullable: false),
                    portfolio = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    platform_id = table.Column<string>(type: "text", nullable: false),
                    platform = table.Column<string>(type: "text", nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "votes",
                schema: "public",
                columns: table => new
                {
                    value = table.Column<long>(type: "bigint", nullable: false),
                    time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_votes_creators_creator_id",
                        column: x => x.creator_id,
                        principalSchema: "public",
                        principalTable: "creators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "creator_applications",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    submitter_id = table.Column<int>(type: "integer", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    platform_id = table.Column<string>(type: "text", nullable: false),
                    ticker = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creator_applications", x => x.id);
                    table.ForeignKey(
                        name: "FK_creator_applications_players_submitter_id",
                        column: x => x.submitter_id,
                        principalSchema: "public",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "loot_boxes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    result_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loot_boxes", x => x.id);
                    table.ForeignKey(
                        name: "FK_loot_boxes_creators_result_id",
                        column: x => x.result_id,
                        principalSchema: "public",
                        principalTable: "creators",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_loot_boxes_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "public",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_portfolios",
                schema: "public",
                columns: table => new
                {
                    value = table.Column<long>(type: "bigint", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_player_portfolios_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "public",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<long>(type: "bigint", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    creator_id = table.Column<int>(type: "integer", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_transactions_creators_creator_id",
                        column: x => x.creator_id,
                        principalSchema: "public",
                        principalTable: "creators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transactions_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "public",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_creator_applications_submitter_id",
                schema: "public",
                table: "creator_applications",
                column: "submitter_id");

            migrationBuilder.CreateIndex(
                name: "IX_creator_opt_outs_platform_id",
                schema: "public",
                table: "creator_opt_outs",
                column: "platform_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_creators_platform_platform_id",
                schema: "public",
                table: "creators",
                columns: new[] { "platform", "platform_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_creators_slug",
                schema: "public",
                table: "creators",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_creators_ticker",
                schema: "public",
                table: "creators",
                column: "ticker",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_loot_boxes_player_id",
                schema: "public",
                table: "loot_boxes",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_loot_boxes_result_id",
                schema: "public",
                table: "loot_boxes",
                column: "result_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_portfolios_player_id_time",
                schema: "public",
                table: "player_portfolios",
                columns: new[] { "player_id", "time" });

            migrationBuilder.CreateIndex(
                name: "IX_players_platform_id",
                schema: "public",
                table: "players",
                column: "platform_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_slug",
                schema: "public",
                table: "players",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_type",
                schema: "public",
                table: "players",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_creator_id",
                schema: "public",
                table: "transactions",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_player_id",
                schema: "public",
                table: "transactions",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_votes_creator_id_time",
                schema: "public",
                table: "votes",
                columns: new[] { "creator_id", "time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "creator_applications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "creator_opt_outs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "loot_boxes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "player_portfolios",
                schema: "public");

            migrationBuilder.DropTable(
                name: "transactions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "votes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "players",
                schema: "public");

            migrationBuilder.DropTable(
                name: "creators",
                schema: "public");
        }
    }
}
