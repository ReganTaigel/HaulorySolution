using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haulory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PickupCompany = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PickupAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DeliveryCompany = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    LoadDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RateType = table.Column<int>(type: "int", nullable: false),
                    RateValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceiverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeliveredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SignatureJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientCompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClientContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClientEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    ClientAddressLine1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ClientCity = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ClientCountry = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    ParentMainUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirthUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Line1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Line2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Suburb = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Postcode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LicenceExpiresOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BusinessEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    BusinessPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BusinessAddress1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    BusinessAddress2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    BusinessSuburb = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BusinessCity = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BusinessRegion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BusinessPostcode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BusinessCountry = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SupplierGstNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupplierNzbn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkSites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Suburb = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Postcode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateOfBirthUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenceNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LicenceVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LicenceClassOrEndorsements = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LicenceIssuedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenceExpiresOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenceConditionsNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmergencyContact_FirstName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    EmergencyContact_LastName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    EmergencyContact_Relationship = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EmergencyContact_PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmergencyContact_SecondaryPhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmergencyContact_Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    Line1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Line2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Suburb = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Postcode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drivers_UserAccounts_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Drivers_UserAccounts_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitNumber = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    VehicleType = table.Column<int>(type: "int", nullable: true),
                    FuelType = table.Column<int>(type: "int", nullable: true),
                    Configuration = table.Column<int>(type: "int", nullable: true),
                    PowerUnitBodyType = table.Column<int>(type: "int", nullable: true),
                    RucOdometerAtPurchaseKm = table.Column<int>(type: "int", nullable: true),
                    RucDistancePurchasedKm = table.Column<int>(type: "int", nullable: true),
                    RucPurchasedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RucNextDueOdometerKm = table.Column<int>(type: "int", nullable: true),
                    RucLicenceStartKm = table.Column<int>(type: "int", nullable: true),
                    RucLicenceEndKm = table.Column<int>(type: "int", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Rego = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RegoExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Make = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CertificateType = table.Column<int>(type: "int", nullable: false),
                    CertificateExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OdometerKm = table.Column<int>(type: "int", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleAssets_UserAccounts_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InductionRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkSiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ValidForDays = table.Column<int>(type: "int", nullable: true),
                    PpeRequired = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InductionRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InductionRequirements_WorkSites_WorkSiteId",
                        column: x => x.WorkSiteId,
                        principalTable: "WorkSites",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VehicleAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    WaitTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    DamageNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveredByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PickupCompany = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PickupAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DeliveryCompany = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LoadDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ClientCompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClientContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClientEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    ClientAddressLine1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ClientCity = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ClientCountry = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RateType = table.Column<int>(type: "int", nullable: false),
                    RateValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantityUnit = table.Column<int>(type: "int", maxLength: 30, nullable: false),
                    ReceiverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliverySignatureJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PickupSignatureJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PickupSignedByName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PickupSignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Jobs_UserAccounts_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jobs_VehicleAssets_VehicleAssetId",
                        column: x => x.VehicleAssetId,
                        principalTable: "VehicleAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OdometerReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitNumber = table.Column<int>(type: "int", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadingKm = table.Column<int>(type: "int", nullable: false),
                    ReadingType = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdometerReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OdometerReadings_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OdometerReadings_UserAccounts_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OdometerReadings_VehicleAssets_VehicleAssetId",
                        column: x => x.VehicleAssetId,
                        principalTable: "VehicleAssets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DriverInductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DriverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkSiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EvidenceFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverInductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverInductions_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DriverInductions_InductionRequirements_RequirementId",
                        column: x => x.RequirementId,
                        principalTable: "InductionRequirements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DriverInductions_WorkSites_WorkSiteId",
                        column: x => x.WorkSiteId,
                        principalTable: "WorkSites",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobTrailerAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrailerAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTrailerAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobTrailerAssignments_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JobTrailerAssignments_VehicleAssets_TrailerAssetId",
                        column: x => x.TrailerAssetId,
                        principalTable: "VehicleAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryReceipts_DeliveredAtUtc",
                table: "DeliveryReceipts",
                column: "DeliveredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryReceipts_OwnerUserId",
                table: "DeliveryReceipts",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryReceipts_OwnerUserId_JobId",
                table: "DeliveryReceipts",
                columns: new[] { "OwnerUserId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverInductions_DriverId",
                table: "DriverInductions",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverInductions_OwnerUserId_DriverId",
                table: "DriverInductions",
                columns: new[] { "OwnerUserId", "DriverId" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverInductions_OwnerUserId_DriverId_WorkSiteId_RequirementId",
                table: "DriverInductions",
                columns: new[] { "OwnerUserId", "DriverId", "WorkSiteId", "RequirementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverInductions_RequirementId",
                table: "DriverInductions",
                column: "RequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverInductions_WorkSiteId",
                table: "DriverInductions",
                column: "WorkSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_OwnerUserId",
                table: "Drivers",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_UserId",
                table: "Drivers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InductionRequirements_OwnerUserId",
                table: "InductionRequirements",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InductionRequirements_OwnerUserId_CompanyName",
                table: "InductionRequirements",
                columns: new[] { "OwnerUserId", "CompanyName" });

            migrationBuilder.CreateIndex(
                name: "IX_InductionRequirements_OwnerUserId_WorkSiteId_IsActive",
                table: "InductionRequirements",
                columns: new[] { "OwnerUserId", "WorkSiteId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_InductionRequirements_WorkSiteId",
                table: "InductionRequirements",
                column: "WorkSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_DriverId",
                table: "Jobs",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OwnerUserId",
                table: "Jobs",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OwnerUserId_InvoiceNumber",
                table: "Jobs",
                columns: new[] { "OwnerUserId", "InvoiceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OwnerUserId_SortOrder",
                table: "Jobs",
                columns: new[] { "OwnerUserId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_VehicleAssetId",
                table: "Jobs",
                column: "VehicleAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTrailerAssignments_JobId",
                table: "JobTrailerAssignments",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTrailerAssignments_JobId_Position",
                table: "JobTrailerAssignments",
                columns: new[] { "JobId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobTrailerAssignments_TrailerAssetId",
                table: "JobTrailerAssignments",
                column: "TrailerAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_OdometerReadings_DriverId",
                table: "OdometerReadings",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_OdometerReadings_RecordedByUserId",
                table: "OdometerReadings",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OdometerReadings_VehicleAssetId",
                table: "OdometerReadings",
                column: "VehicleAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_OdometerReadings_VehicleAssetId_UnitNumber_RecordedAtUtc",
                table: "OdometerReadings",
                columns: new[] { "VehicleAssetId", "UnitNumber", "RecordedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Email",
                table: "UserAccounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_ParentMainUserId",
                table: "UserAccounts",
                column: "ParentMainUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssets_OwnerUserId",
                table: "VehicleAssets",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssets_OwnerUserId_Rego",
                table: "VehicleAssets",
                columns: new[] { "OwnerUserId", "Rego" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssets_VehicleSetId",
                table: "VehicleAssets",
                column: "VehicleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssets_VehicleSetId_UnitNumber",
                table: "VehicleAssets",
                columns: new[] { "VehicleSetId", "UnitNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSites_OwnerUserId",
                table: "WorkSites",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSites_OwnerUserId_CompanyName",
                table: "WorkSites",
                columns: new[] { "OwnerUserId", "CompanyName" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkSites_OwnerUserId_IsActive",
                table: "WorkSites",
                columns: new[] { "OwnerUserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkSites_OwnerUserId_Name",
                table: "WorkSites",
                columns: new[] { "OwnerUserId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryReceipts");

            migrationBuilder.DropTable(
                name: "DriverInductions");

            migrationBuilder.DropTable(
                name: "JobTrailerAssignments");

            migrationBuilder.DropTable(
                name: "OdometerReadings");

            migrationBuilder.DropTable(
                name: "InductionRequirements");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "WorkSites");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "VehicleAssets");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}
