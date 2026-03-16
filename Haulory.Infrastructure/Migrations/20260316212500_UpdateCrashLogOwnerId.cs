using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haulory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCrashLogOwnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ServerCrashLogs",
                newName: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "ServerCrashLogs",
                newName: "UserId");
        }
    }
}
