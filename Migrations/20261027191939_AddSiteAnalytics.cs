using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcobayApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AnalyticsVisitId",
                table: "Requests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnalyticsEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    DailyHost = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    AmountFrom = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    AmountTo = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstalledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsVisits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    CityName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LandingPath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Campaign = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Term = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReferrerHost = table.Column<string>(type: "nvarchar(253)", maxLength: 253, nullable: false),
                    Device = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Browser = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsVisits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_AnalyticsVisitId",
                table: "Requests",
                column: "AnalyticsVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_CreatedAtUtc_Kind",
                table: "AnalyticsEvents",
                columns: new[] { "CreatedAtUtc", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_VisitId_CreatedAtUtc",
                table: "AnalyticsEvents",
                columns: new[] { "VisitId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsVisits_StartedAtUtc_CityId",
                table: "AnalyticsVisits",
                columns: new[] { "StartedAtUtc", "CityId" });
            migrationBuilder.Sql("INSERT INTO [AnalyticsStates] ([InstalledAtUtc]) VALUES (SYSUTCDATETIME());");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsEvents");

            migrationBuilder.DropTable(
                name: "AnalyticsStates");

            migrationBuilder.DropTable(
                name: "AnalyticsVisits");

            migrationBuilder.DropIndex(
                name: "IX_Requests_AnalyticsVisitId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "AnalyticsVisitId",
                table: "Requests");
        }
    }
}
