using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductCatalogue.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Products_ProductId",
                table: "Assets");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Products_ProductId",
                table: "Assets",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Products_ProductId",
                table: "Assets");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Products_ProductId",
                table: "Assets",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
