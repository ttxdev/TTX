using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTX.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class Cleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loot_boxes_creators_result_id",
                schema: "public",
                table: "loot_boxes");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "stream_started_at",
                schema: "public",
                table: "creators",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "stream_ended_at",
                schema: "public",
                table: "creators",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_loot_boxes_creators_result_id",
                schema: "public",
                table: "loot_boxes",
                column: "result_id",
                principalSchema: "public",
                principalTable: "creators",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loot_boxes_creators_result_id",
                schema: "public",
                table: "loot_boxes");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "stream_started_at",
                schema: "public",
                table: "creators",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "stream_ended_at",
                schema: "public",
                table: "creators",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_loot_boxes_creators_result_id",
                schema: "public",
                table: "loot_boxes",
                column: "result_id",
                principalSchema: "public",
                principalTable: "creators",
                principalColumn: "id");
        }
    }
}
