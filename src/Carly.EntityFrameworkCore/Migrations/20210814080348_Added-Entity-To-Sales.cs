using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class AddedEntityToSales : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaimedVoucher",
                table: "Sales",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaimedVoucher",
                table: "Sales");
        }
    }
}
