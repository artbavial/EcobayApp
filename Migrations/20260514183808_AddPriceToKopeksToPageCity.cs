using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcobayApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToKopeksToPageCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriceToKopeks",
                table: "PageCities",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceToKopeks",
                table: "PageCities");
        }
    }
}
