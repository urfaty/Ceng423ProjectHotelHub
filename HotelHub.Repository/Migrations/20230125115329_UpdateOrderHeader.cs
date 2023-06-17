using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelHub.Repository.Migrations
{
    public partial class UpdateOrderHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateofPayment",
                table: "OrderHeaders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateofPayment",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "OrderHeaders");
        }
    }
}
