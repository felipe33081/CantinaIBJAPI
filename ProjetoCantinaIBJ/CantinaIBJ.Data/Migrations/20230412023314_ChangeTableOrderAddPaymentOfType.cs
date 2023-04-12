using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CantinaIBJ.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTableOrderAddPaymentOfType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentOfType",
                table: "Order",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentOfType",
                table: "Order");
        }
    }
}
