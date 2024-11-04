using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class updateSellTableProfit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Profit",
                table: "Sell",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Buy",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Profit",
                table: "Sell");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Buy",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
