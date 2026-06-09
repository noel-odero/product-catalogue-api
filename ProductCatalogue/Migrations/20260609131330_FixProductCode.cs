using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductCatalogue.Migrations
{
    /// <inheritdoc />
    public partial class FixProductCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductionCode",
                table: "Products",
                newName: "ProductCode");

            migrationBuilder.RenameIndex(
                name: "IX_Products_ProductionCode",
                table: "Products",
                newName: "IX_Products_ProductCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductCode",
                table: "Products",
                newName: "ProductionCode");

            migrationBuilder.RenameIndex(
                name: "IX_Products_ProductCode",
                table: "Products",
                newName: "IX_Products_ProductionCode");
        }
    }
}
