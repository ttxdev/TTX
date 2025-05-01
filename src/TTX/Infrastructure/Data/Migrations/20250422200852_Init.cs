using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TTX.Infrastructure.Data.Migrations
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
                name: "creators",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    ticker = table.Column<string>(type: "text", nullable: false),
                    twitch_id = table.Column<string>(type: "text", nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<long>(type: "bigint", nullable: false),
                    stream_is_live = table.Column<bool>(type: "boolean", nullable: false),
                    stream_started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    stream_ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    twitch_id = table.Column<string>(type: "text", nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: false),
                    credits = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    creator_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<long>(type: "bigint", nullable: false),
                    time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                name: "transactions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<long>(type: "bigint", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    creator_id = table.Column<int>(type: "integer", nullable: false),
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
                name: "IX_creators_twitch_id",
                schema: "public",
                table: "creators",
                column: "twitch_id",
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
                name: "IX_players_slug",
                schema: "public",
                table: "players",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_twitch_id",
                schema: "public",
                table: "players",
                column: "twitch_id",
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
                name: "loot_boxes",
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