using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CantinaIBJ.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveColumInCustomerPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditBalance",
                table: "CustomerPerson");

            migrationBuilder.RenameColumn(
                name: "DebitBalance",
                table: "CustomerPerson",
                newName: "Balance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "CustomerPerson",
                newName: "DebitBalance");

            migrationBuilder.AddColumn<decimal>(
                name: "CreditBalance",
                table: "CustomerPerson",
                type: "numeric",
                nullable: true);
        }
    }
}
