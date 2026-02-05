using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TTX.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseDoublesForMoney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "value",
                schema: "public",
                table: "votes",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<double>(
                name: "value",
                schema: "public",
                table: "transactions",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<double>(
                name: "portfolio",
                schema: "public",
                table: "players",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<double>(
                name: "credits",
                schema: "public",
                table: "players",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<double>(
                name: "value",
                schema: "public",
                table: "player_portfolios",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<double>(
                name: "value",
                schema: "public",
                table: "creators",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "value",
                schema: "public",
                table: "votes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<long>(
                name: "value",
                schema: "public",
                table: "transactions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<long>(
                name: "portfolio",
                schema: "public",
                table: "players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<long>(
                name: "credits",
                schema: "public",
                table: "players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<long>(
                name: "value",
                schema: "public",
                table: "player_portfolios",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<long>(
                name: "value",
                schema: "public",
                table: "creators",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
