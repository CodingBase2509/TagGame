using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagGame.Api.Core.Persistence.Migrations.Games
{
    /// <inheritdoc />
    public partial class InitGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "games");

            migrationBuilder.CreateTable(
                name: "game_rooms",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccessCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Boundaries = table.Column<string>(type: "jsonb", nullable: true),
                    Settings = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "matches",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentRoundNo = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_matches_game_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "games",
                        principalTable: "game_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "room_memberships",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    PermissionsMask = table.Column<int>(type: "integer", nullable: false),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_room_memberships_game_rooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "games",
                        principalTable: "game_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rounds",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundNo = table.Column<int>(type: "integer", nullable: false),
                    Phase = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rounds_matches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "games",
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_rooms_AccessCode",
                schema: "games",
                table: "game_rooms",
                column: "AccessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_rooms_OwnerUserId",
                schema: "games",
                table: "game_rooms",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_matches_RoomId",
                schema: "games",
                table: "matches",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_room_memberships_RoomId",
                schema: "games",
                table: "room_memberships",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_room_memberships_UserId_RoomId",
                schema: "games",
                table: "room_memberships",
                columns: new[] { "UserId", "RoomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rounds_MatchId",
                schema: "games",
                table: "rounds",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_rounds_MatchId_RoundNo",
                schema: "games",
                table: "rounds",
                columns: new[] { "MatchId", "RoundNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "room_memberships",
                schema: "games");

            migrationBuilder.DropTable(
                name: "rounds",
                schema: "games");

            migrationBuilder.DropTable(
                name: "matches",
                schema: "games");

            migrationBuilder.DropTable(
                name: "game_rooms",
                schema: "games");
        }
    }
}
