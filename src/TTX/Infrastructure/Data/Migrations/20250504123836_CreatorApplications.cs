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
            migrationBuilder.DropPrimaryKey(
                name: "PK_applications",
                schema: "public",
                table: "applications");

            migrationBuilder.RenameTable(
                name: "applications",
                schema: "public",
                newName: "creator_applications",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "creator_slug",
                schema: "public",
                table: "creator_applications",
                newName: "twitch_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_creator_applications",
                schema: "public",
                table: "creator_applications",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_creator_applications",
                schema: "public",
                table: "creator_applications");

            migrationBuilder.RenameTable(
                name: "creator_applications",
                schema: "public",
                newName: "applications",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "twitch_id",
                schema: "public",
                table: "applications",
                newName: "creator_slug");

            migrationBuilder.AddPrimaryKey(
                name: "PK_applications",
                schema: "public",
                table: "applications",
                column: "id");
        }
    }
}
