using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagGame.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGameRoomProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Rooms",
                newName: "OwnerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerUserId",
                table: "Rooms",
                newName: "CreatorId");
        }
    }
}
