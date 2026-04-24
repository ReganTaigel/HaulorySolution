using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaulitCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWaitAndUnloadChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HandUnloadCharge",
                table: "DocumentSettings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "HandUnloadChargeEnabled",
                table: "DocumentSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "WaitTimeCharge",
                table: "DocumentSettings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "WaitTimeChargeEnabled",
                table: "DocumentSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandUnloadCharge",
                table: "DocumentSettings");

            migrationBuilder.DropColumn(
                name: "HandUnloadChargeEnabled",
                table: "DocumentSettings");

            migrationBuilder.DropColumn(
                name: "WaitTimeCharge",
                table: "DocumentSettings");

            migrationBuilder.DropColumn(
                name: "WaitTimeChargeEnabled",
                table: "DocumentSettings");
        }
    }
}
