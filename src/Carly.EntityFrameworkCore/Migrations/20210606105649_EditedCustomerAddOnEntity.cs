using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class EditedCustomerAddOnEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerAddOns_CustomerPrincipals_CustomerPrincipalId",
                table: "CustomerAddOns");

            migrationBuilder.DropColumn(
                name: "PrincipalId",
                table: "CustomerAddOns");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerPrincipalId",
                table: "CustomerAddOns",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerAddOns_CustomerPrincipals_CustomerPrincipalId",
                table: "CustomerAddOns",
                column: "CustomerPrincipalId",
                principalTable: "CustomerPrincipals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerAddOns_CustomerPrincipals_CustomerPrincipalId",
                table: "CustomerAddOns");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerPrincipalId",
                table: "CustomerAddOns",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PrincipalId",
                table: "CustomerAddOns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerAddOns_CustomerPrincipals_CustomerPrincipalId",
                table: "CustomerAddOns",
                column: "CustomerPrincipalId",
                principalTable: "CustomerPrincipals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
