using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTX.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatorApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "creator_applications",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone")
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "twitch_id",
                schema: "public",
                table: "creator_applications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<string>(
                name: "ticker",
                schema: "public",
                table: "creator_applications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "public",
                table: "creator_applications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "public",
                table: "creator_applications",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<int>(
                name: "submitter_id",
                schema: "public",
                table: "creator_applications",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.CreateIndex(
                name: "IX_creator_applications_submitter_id",
                schema: "public",
                table: "creator_applications",
                column: "submitter_id");

            migrationBuilder.AddForeignKey(
                name: "FK_creator_applications_players_submitter_id",
                schema: "public",
                table: "creator_applications",
                column: "submitter_id",
                principalSchema: "public",
                principalTable: "players",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_creator_applications_players_submitter_id",
                schema: "public",
                table: "creator_applications");

            migrationBuilder.DropIndex(
                name: "IX_creator_applications_submitter_id",
                schema: "public",
                table: "creator_applications");

            migrationBuilder.DropColumn(
                name: "submitter_id",
                schema: "public",
                table: "creator_applications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "public",
                table: "creator_applications",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "twitch_id",
                schema: "public",
                table: "creator_applications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Relational:ColumnOrder", 1)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "ticker",
                schema: "public",
                table: "creator_applications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Relational:ColumnOrder", 2)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "public",
                table: "creator_applications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text")
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "public",
                table: "creator_applications",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 5);
        }
    }
}
