using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcobayApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCityLocalContentAndRequestIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverageText",
                table: "PageCities",
                type: "nvarchar(1500)",
                maxLength: 1500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroTitle",
                table: "PageCities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalIntro",
                table: "PageCities",
                type: "nvarchar(1500)",
                maxLength: 1500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupDetails",
                table: "PageCities",
                type: "nvarchar(1500)",
                maxLength: 1500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_DateCreate_Id",
                table: "Requests",
                columns: new[] { "DateCreate", "Id" });

            // Replace only known old defaults. Do not overwrite the owner's custom copy.
            migrationBuilder.Sql("""
                UPDATE [MainPageSettings]
                SET [Title] = N'Вывоз техники в наших городах и их районах'
                WHERE [Title] = N'Вывоз бытовой техники по всей Беларуси';
                UPDATE [MainPageSettings]
                SET [HeroTitle] = N'Вывоз техники в наших городах и их районах'
                WHERE [HeroTitle] = N'Быстрый вывоз техники на утилизацию по всей Беларуси';
                UPDATE [MainPageSettings]
                SET [Description] = N'Вывоз техники только в указанных на сайте городах и населённых пунктах их районов. Выберите город и узнайте условия.'
                WHERE [Description] = N'Вывоз и утилизация бытовой техники по Беларуси: приедем, вынесем, вывезем и заплатим на месте.';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_DateCreate_Id",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CoverageText",
                table: "PageCities");

            migrationBuilder.DropColumn(
                name: "HeroTitle",
                table: "PageCities");

            migrationBuilder.DropColumn(
                name: "LocalIntro",
                table: "PageCities");

            migrationBuilder.DropColumn(
                name: "PickupDetails",
                table: "PageCities");
        }
    }
}
