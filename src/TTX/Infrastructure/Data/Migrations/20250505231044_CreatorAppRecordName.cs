using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTX.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatorAppRecordName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubmitterId1",
                schema: "public",
                table: "creator_applications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "public",
                table: "creator_applications",
                type: "text",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.CreateIndex(
                name: "IX_creator_applications_SubmitterId1",
                schema: "public",
                table: "creator_applications",
                column: "SubmitterId1");

            migrationBuilder.AddForeignKey(
                name: "FK_creator_applications_players_SubmitterId1",
                schema: "public",
                table: "creator_applications",
                column: "SubmitterId1",
                principalSchema: "public",
                principalTable: "players",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_creator_applications_players_SubmitterId1",
                schema: "public",
                table: "creator_applications");

            migrationBuilder.DropIndex(
                name: "IX_creator_applications_SubmitterId1",
                schema: "public",
                table: "creator_applications");

            migrationBuilder.DropColumn(
                name: "SubmitterId1",
                schema: "public",
                table: "creator_applications");

            migrationBuilder.DropColumn(
                name: "name",
                schema: "public",
                table: "creator_applications");
        }
    }
}
