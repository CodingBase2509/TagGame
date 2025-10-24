using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagGame.Api.Core.Persistence.Migrations.Games
{
    /// <inheritdoc />
    public partial class AddPlayerType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "games",
                table: "room_memberships",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                schema: "games",
                table: "room_memberships");
        }
    }
}
