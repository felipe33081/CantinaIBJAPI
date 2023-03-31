using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CantinaIBJ.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTableProductHistoric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductHistoric_Product_ProductId",
                table: "ProductHistoric");

            migrationBuilder.DropIndex(
                name: "IX_ProductHistoric_ProductId",
                table: "ProductHistoric");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductHistoric_ProductId",
                table: "ProductHistoric",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductHistoric_Product_ProductId",
                table: "ProductHistoric",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
