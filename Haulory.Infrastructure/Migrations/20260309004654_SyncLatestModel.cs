using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haulory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncLatestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobTrailerAssignments_Jobs_JobId",
                table: "JobTrailerAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_JobTrailerAssignments_JobId_TrailerAssetId",
                table: "JobTrailerAssignments",
                columns: new[] { "JobId", "TrailerAssetId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobTrailerAssignments_Jobs_JobId",
                table: "JobTrailerAssignments",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobTrailerAssignments_Jobs_JobId",
                table: "JobTrailerAssignments");

            migrationBuilder.DropIndex(
                name: "IX_JobTrailerAssignments_JobId_TrailerAssetId",
                table: "JobTrailerAssignments");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTrailerAssignments_Jobs_JobId",
                table: "JobTrailerAssignments",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id");
        }
    }
}
