using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcobayApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesRequestWorkflowFeedbackAutoGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Requests",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Requests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverComment",
                table: "Requests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManual",
                table: "Requests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "Requests",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "Requests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessedByName",
                table: "Requests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProcessedByUserId",
                table: "Requests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScreenshotPath",
                table: "Requests",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "AutoIncrementEnabled",
                table: "Goals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AutoIncrementFromHour",
                table: "Goals",
                type: "int",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<DateTime>(
                name: "AutoIncrementLastAt",
                table: "Goals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AutoIncrementPerHour",
                table: "Goals",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "AutoIncrementToHour",
                table: "Goals",
                type: "int",
                nullable: false,
                defaultValue: 21);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AdminUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Admin");

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SourcePage = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    AdminComment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RequestEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PaidAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestEvents_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status_DateCreate",
                table: "Requests",
                columns: new[] { "Status", "DateCreate" });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CityId",
                table: "Feedbacks",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CreatedAt",
                table: "Feedbacks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RequestEvents_RequestId",
                table: "RequestEvents",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "RequestEvents");

            migrationBuilder.DropIndex(
                name: "IX_Requests_Status_DateCreate",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DriverComment",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "IsManual",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ProcessedByName",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ProcessedByUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ScreenshotPath",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "AutoIncrementEnabled",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "AutoIncrementFromHour",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "AutoIncrementLastAt",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "AutoIncrementPerHour",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "AutoIncrementToHour",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AdminUsers");
        }
    }
}
