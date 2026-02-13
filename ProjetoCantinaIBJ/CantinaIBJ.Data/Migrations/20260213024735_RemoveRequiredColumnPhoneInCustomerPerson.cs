using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CantinaIBJ.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequiredColumnPhoneInCustomerPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "CustomerPerson",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPerson_Name",
                table: "CustomerPerson",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerPerson_Name",
                table: "CustomerPerson");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "CustomerPerson",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
