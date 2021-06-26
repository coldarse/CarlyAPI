using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class EditedCustomerAddOnEntity2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "CustomerAddOns");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "CustomerAddOns",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
