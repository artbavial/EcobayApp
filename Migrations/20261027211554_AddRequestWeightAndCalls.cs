using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcobayApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestWeightAndCalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CustomerCalledAtUtc",
                table: "Requests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCalledByName",
                table: "Requests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CustomerCalledByUserId",
                table: "Requests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "Requests",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "RequestEvents",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RequestContactEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    IsCalled = table.Column<bool>(type: "bit", nullable: false),
                    CallAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestContactEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestContactEvents_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status_ProcessedAt",
                table: "Requests",
                columns: new[] { "Status", "ProcessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestContactEvents_RequestId_CreatedAtUtc",
                table: "RequestContactEvents",
                columns: new[] { "RequestId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestContactEvents");

            migrationBuilder.DropIndex(
                name: "IX_Requests_Status_ProcessedAt",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CustomerCalledAtUtc",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CustomerCalledByName",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CustomerCalledByUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "RequestEvents");
        }
    }
}
