using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HaulitCore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryReceiptInvoiceExtras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HandUnloadChargeAmount",
                table: "DeliveryReceipts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "HandUnloadChargeEnabled",
                table: "DeliveryReceipts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "WaitTimeChargeAmount",
                table: "DeliveryReceipts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "WaitTimeChargeEnabled",
                table: "DeliveryReceipts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandUnloadChargeAmount",
                table: "DeliveryReceipts");

            migrationBuilder.DropColumn(
                name: "HandUnloadChargeEnabled",
                table: "DeliveryReceipts");

            migrationBuilder.DropColumn(
                name: "WaitTimeChargeAmount",
                table: "DeliveryReceipts");

            migrationBuilder.DropColumn(
                name: "WaitTimeChargeEnabled",
                table: "DeliveryReceipts");
        }
    }
}
