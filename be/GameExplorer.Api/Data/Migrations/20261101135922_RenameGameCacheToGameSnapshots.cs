using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameExplorer.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameGameCacheToGameSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_game_cache",
                table: "game_cache");

            migrationBuilder.DropIndex(
                name: "ix_game_cache_slug",
                table: "game_cache");

            migrationBuilder.DropCheckConstraint(
                name: "ck_game_cache_metacritic",
                table: "game_cache");

            migrationBuilder.DropCheckConstraint(
                name: "ck_game_cache_rating",
                table: "game_cache");

            migrationBuilder.RenameTable(
                name: "game_cache",
                newName: "game_snapshots");

            migrationBuilder.AddPrimaryKey(
                name: "pk_game_snapshots",
                table: "game_snapshots",
                column: "rawg_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_game_snapshots_metacritic",
                table: "game_snapshots",
                sql: "metacritic BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "ck_game_snapshots_rating",
                table: "game_snapshots",
                sql: "rating BETWEEN 0 AND 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_game_snapshots",
                table: "game_snapshots");

            migrationBuilder.DropCheckConstraint(
                name: "ck_game_snapshots_metacritic",
                table: "game_snapshots");

            migrationBuilder.DropCheckConstraint(
                name: "ck_game_snapshots_rating",
                table: "game_snapshots");

            migrationBuilder.RenameTable(
                name: "game_snapshots",
                newName: "game_cache");

            migrationBuilder.AddPrimaryKey(
                name: "pk_game_cache",
                table: "game_cache",
                column: "rawg_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_cache_slug",
                table: "game_cache",
                column: "slug",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_game_cache_metacritic",
                table: "game_cache",
                sql: "metacritic BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "ck_game_cache_rating",
                table: "game_cache",
                sql: "rating BETWEEN 0 AND 5");
        }
    }
}
