using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class EditedVoucherEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Vouchers_AddOns_giftId",
            //    table: "Vouchers");

            //migrationBuilder.DropIndex(
            //    name: "IX_Vouchers_giftId",
            //    table: "Vouchers");

            migrationBuilder.AlterColumn<int>(
                name: "giftId",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "giftId",
                table: "Vouchers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Vouchers_giftId",
            //    table: "Vouchers",
            //    column: "giftId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Vouchers_AddOns_giftId",
            //    table: "Vouchers",
            //    column: "giftId",
            //    principalTable: "AddOns",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }
    }
}
