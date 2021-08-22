using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class Added_ImageLink_To_CustomerPrincipals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageLink",
                table: "CustomerPrincipals",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageLink",
                table: "CustomerPrincipals");
        }
    }
}
