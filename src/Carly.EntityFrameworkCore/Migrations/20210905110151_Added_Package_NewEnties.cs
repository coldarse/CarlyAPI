using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class Added_Package_NewEnties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "AdminFee",
                table: "Packages",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Delivery",
                table: "Packages",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Roadtax",
                table: "Packages",
                type: "float",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminFee",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "Delivery",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "Roadtax",
                table: "Packages");
        }
    }
}
