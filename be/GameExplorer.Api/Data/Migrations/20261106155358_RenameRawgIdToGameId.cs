using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameExplorer.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameRawgIdToGameId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM game_snapshots;");
            migrationBuilder.RenameColumn(
                name: "rawg_id",
                table: "game_snapshots",
                newName: "game_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "game_id",
                table: "game_snapshots",
                newName: "rawg_id");
        }
    }
}
