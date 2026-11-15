using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameExplorer.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropGameSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_favorites_game_snapshots_game_id",
                table: "favorites");

            migrationBuilder.DropForeignKey(
                name: "fk_wishlist_game_snapshots_game_id",
                table: "wishlist");

            migrationBuilder.DropTable(
                name: "game_snapshots");

            migrationBuilder.DropIndex(
                name: "ix_wishlist_game_id",
                table: "wishlist");

            migrationBuilder.DropIndex(
                name: "ix_favorites_game_id",
                table: "favorites");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_snapshots",
                columns: table => new
                {
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    image = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    rating = table.Column<double>(type: "double precision", nullable: false),
                    release_date = table.Column<DateOnly>(type: "date", nullable: true),
                    slug = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    genres = table.Column<string>(type: "jsonb", nullable: false),
                    platforms = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_snapshots", x => x.game_id);
                    table.CheckConstraint("ck_game_snapshots_rating", "rating BETWEEN 0 AND 100");
                });

            migrationBuilder.CreateIndex(
                name: "ix_wishlist_game_id",
                table: "wishlist",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorites_game_id",
                table: "favorites",
                column: "game_id");

            migrationBuilder.AddForeignKey(
                name: "fk_favorites_game_snapshots_game_id",
                table: "favorites",
                column: "game_id",
                principalTable: "game_snapshots",
                principalColumn: "game_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_wishlist_game_snapshots_game_id",
                table: "wishlist",
                column: "game_id",
                principalTable: "game_snapshots",
                principalColumn: "game_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
