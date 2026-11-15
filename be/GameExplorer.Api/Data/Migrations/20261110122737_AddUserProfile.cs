using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameExplorer.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False");

            migrationBuilder.AddColumn<string>(
                name: "avatar",
                table: "AspNetUsers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "display_name",
                table: "AspNetUsers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                collation: "case_insensitive");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_display_name",
                table: "AspNetUsers",
                column: "display_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_asp_net_users_display_name",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "avatar",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "display_name",
                table: "AspNetUsers");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:CollationDefinition:case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False");
        }
    }
}
