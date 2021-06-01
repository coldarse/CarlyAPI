using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class EditedAddOnEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "AddOns",
                newName: "addonname");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "addonname",
                table: "AddOns",
                newName: "name");
        }
    }
}
