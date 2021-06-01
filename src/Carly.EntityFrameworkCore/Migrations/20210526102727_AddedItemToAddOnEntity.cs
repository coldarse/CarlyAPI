using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class AddedItemToAddOnEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddOns_Principals_PrincipalId",
                table: "AddOns");

            migrationBuilder.AlterColumn<int>(
                name: "PrincipalId",
                table: "AddOns",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AddOns_Principals_PrincipalId",
                table: "AddOns",
                column: "PrincipalId",
                principalTable: "Principals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddOns_Principals_PrincipalId",
                table: "AddOns");

            migrationBuilder.AlterColumn<int>(
                name: "PrincipalId",
                table: "AddOns",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_AddOns_Principals_PrincipalId",
                table: "AddOns",
                column: "PrincipalId",
                principalTable: "Principals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
