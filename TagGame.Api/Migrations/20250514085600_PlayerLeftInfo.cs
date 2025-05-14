using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagGame.Api.Migrations
{
    /// <inheritdoc />
    public partial class PlayerLeftInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayLeftInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisconnectType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayLeftInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayLeftInfo_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayLeftInfo_PlayerId",
                table: "PlayLeftInfo",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayLeftInfo");
        }
    }
}
