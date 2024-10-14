using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class updateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Shop_ClintId",
                table: "Product");

            migrationBuilder.RenameColumn(
                name: "ClintId",
                table: "Product",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_ClintId",
                table: "Product",
                newName: "IX_Product_EmployeeId");

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddDebtAmounts",
                table: "Client",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddDebtDates",
                table: "Client",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Shop_EmployeeId",
                table: "Product",
                column: "EmployeeId",
                principalTable: "Shop",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Shop_EmployeeId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "AddDebtAmounts",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "AddDebtDates",
                table: "Client");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Product",
                newName: "ClintId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_EmployeeId",
                table: "Product",
                newName: "IX_Product_ClintId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Shop_ClintId",
                table: "Product",
                column: "ClintId",
                principalTable: "Shop",
                principalColumn: "Id");
        }
    }
}
