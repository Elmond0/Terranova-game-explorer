using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameExplorer.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGameCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_cache",
                columns: table => new
                {
                    rawg_id = table.Column<int>(type: "integer", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    image = table.Column<string>(type: "text", nullable: true),
                    release_date = table.Column<DateOnly>(type: "date", nullable: true),
                    rating = table.Column<double>(type: "double precision", nullable: false),
                    metacritic = table.Column<int>(type: "integer", nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_cache", x => x.rawg_id);
                    table.CheckConstraint("ck_game_cache_metacritic", "metacritic BETWEEN 0 AND 100");
                    table.CheckConstraint("ck_game_cache_rating", "rating BETWEEN 0 AND 5");
                });

            migrationBuilder.CreateIndex(
                name: "ix_game_cache_slug",
                table: "game_cache",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_cache");
        }
    }
}
