using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameExplorer.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMetacriticRatingTo100 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_game_snapshots_metacritic",
                table: "game_snapshots");

            migrationBuilder.DropCheckConstraint(
                name: "ck_game_snapshots_rating",
                table: "game_snapshots");

            migrationBuilder.DropColumn(
                name: "metacritic",
                table: "game_snapshots");

            migrationBuilder.AddCheckConstraint(
                name: "ck_game_snapshots_rating",
                table: "game_snapshots",
                sql: "rating BETWEEN 0 AND 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_game_snapshots_rating",
                table: "game_snapshots");

            migrationBuilder.AddColumn<int>(
                name: "metacritic",
                table: "game_snapshots",
                type: "integer",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_game_snapshots_metacritic",
                table: "game_snapshots",
                sql: "metacritic BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "ck_game_snapshots_rating",
                table: "game_snapshots",
                sql: "rating BETWEEN 0 AND 5");
        }
    }
}
