using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcobayApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeliveryPriceModeToPageCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeliveryPriceMode",
                table: "PageCities",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeliveryPriceMode",
                table: "PageCities");
        }
    }
}
