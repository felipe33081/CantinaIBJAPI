using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CantinaIBJ.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsInOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChangeValue",
                table: "Order",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentValue",
                table: "Order",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeValue",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PaymentValue",
                table: "Order");
        }
    }
}
