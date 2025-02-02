﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class EditedCustomerPrincipalEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Premium",
                table: "CustomerPrincipals",
                type: "float",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Premium",
                table: "CustomerPrincipals");
        }
    }
}
