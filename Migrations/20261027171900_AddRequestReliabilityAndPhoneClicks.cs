using System;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace EcobayApp.Migrations;

public partial class AddRequestReliabilityAndPhoneClicks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "RequestSubmissions", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            RequestId = table.Column<long>(type: "bigint", nullable: false),
            Fingerprint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
            CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_RequestSubmissions", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_RequestSubmissions_Fingerprint_CreatedAtUtc", table: "RequestSubmissions", columns: new[] { "Fingerprint", "CreatedAtUtc" });

        migrationBuilder.CreateTable(name: "RequestEmailOutbox", columns: table => new
        {
            Id = table.Column<long>(type: "bigint", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            RequestId = table.Column<long>(type: "bigint", nullable: false),
            PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Attempts = table.Column<int>(type: "int", nullable: false),
            AvailableAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
            LeaseUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
            SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
            LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
        }, constraints: table => table.PrimaryKey("PK_RequestEmailOutbox", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_RequestEmailOutbox_RequestId", table: "RequestEmailOutbox", column: "RequestId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_RequestEmailOutbox_SentAtUtc_AvailableAtUtc", table: "RequestEmailOutbox", columns: new[] { "SentAtUtc", "AvailableAtUtc" });

        migrationBuilder.CreateTable(name: "PhoneClicks", columns: table => new
        {
            Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CityId = table.Column<long>(type: "bigint", nullable: true),
            CityName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
            Path = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
            CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_PhoneClicks", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_PhoneClicks_CreatedAtUtc_CityId", table: "PhoneClicks", columns: new[] { "CreatedAtUtc", "CityId" });
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("PhoneClicks");
        migrationBuilder.DropTable("RequestEmailOutbox");
        migrationBuilder.DropTable("RequestSubmissions");
    }
}
