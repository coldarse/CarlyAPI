using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class Added_CustomerPrincipal_NewEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Excess",
                table: "CustomerPrincipals",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "GrossPremium",
                table: "CustomerPrincipals",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Loading1",
                table: "CustomerPrincipals",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Loading2",
                table: "CustomerPrincipals",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "NCDA",
                table: "CustomerPrincipals",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "NCDP",
                table: "CustomerPrincipals",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "SumInsured",
                table: "CustomerPrincipals",
                type: "float",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Excess",
                table: "CustomerPrincipals");

            migrationBuilder.DropColumn(
                name: "GrossPremium",
                table: "CustomerPrincipals");

            migrationBuilder.DropColumn(
                name: "Loading1",
                table: "CustomerPrincipals");

            migrationBuilder.DropColumn(
                name: "Loading2",
                table: "CustomerPrincipals");

            migrationBuilder.DropColumn(
                name: "NCDA",
                table: "CustomerPrincipals");

            migrationBuilder.DropColumn(
                name: "NCDP",
                table: "CustomerPrincipals");

            migrationBuilder.DropColumn(
                name: "SumInsured",
                table: "CustomerPrincipals");
        }
    }
}
