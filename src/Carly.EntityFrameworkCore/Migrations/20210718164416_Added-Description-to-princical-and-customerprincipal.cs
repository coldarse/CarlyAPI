using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class AddedDescriptiontoprincicalandcustomerprincipal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Principals",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CustomerPrincipals",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Principals");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CustomerPrincipals");
        }
    }
}
