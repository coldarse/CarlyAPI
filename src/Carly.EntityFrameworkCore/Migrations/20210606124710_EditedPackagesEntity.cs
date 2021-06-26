using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class EditedPackagesEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VegicleRegNo",
                table: "Packages",
                newName: "VehicleRegNo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VehicleRegNo",
                table: "Packages",
                newName: "VegicleRegNo");
        }
    }
}
