using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class ClientProductIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "Product");

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "Product",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_ClientId",
                table: "Product",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Client_ClientId",
                table: "Product",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Client_ClientId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_ClientId",
                table: "Product");

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
