using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcobayApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteSheetsAndPickupTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PickupDate",
                table: "Requests",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PickupTime",
                table: "Requests",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RouteDate",
                table: "Requests",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RouteDriverId",
                table: "Requests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RouteOrder",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RouteDriverId_RouteDate",
                table: "Requests",
                columns: new[] { "RouteDriverId", "RouteDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_RouteDriverId_RouteDate",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "PickupDate",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "PickupTime",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RouteDate",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RouteDriverId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RouteOrder",
                table: "Requests");
        }
    }
}
