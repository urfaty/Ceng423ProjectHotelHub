using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelHub.Repository.Migrations
{
    public partial class PaymentInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransId",
                table: "OrderHeaders",
                newName: "PaymentIntentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentIntentId",
                table: "OrderHeaders",
                newName: "TransId");
        }
    }
}
