using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagGame.Api.Core.Persistence.Migrations.Games
{
    /// <inheritdoc />
    public partial class GameRoomETag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "games",
                table: "game_rooms",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "games",
                table: "game_rooms");
        }
    }
}
