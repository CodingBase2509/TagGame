using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagGame.Api.Core.Persistence.Migrations.Auth
{
    /// <inheritdoc />
    public partial class UserETag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "auth",
                table: "users",
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
                schema: "auth",
                table: "users");
        }
    }
}
