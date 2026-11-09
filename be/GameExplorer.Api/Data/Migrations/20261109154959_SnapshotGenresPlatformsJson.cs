using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameExplorer.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotGenresPlatformsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "details",
                table: "game_snapshots");

            migrationBuilder.AddColumn<string>(
                name: "genres",
                table: "game_snapshots",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "platforms",
                table: "game_snapshots",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "genres",
                table: "game_snapshots");

            migrationBuilder.DropColumn(
                name: "platforms",
                table: "game_snapshots");

            migrationBuilder.AddColumn<string>(
                name: "details",
                table: "game_snapshots",
                type: "jsonb",
                nullable: true);
        }
    }
}
