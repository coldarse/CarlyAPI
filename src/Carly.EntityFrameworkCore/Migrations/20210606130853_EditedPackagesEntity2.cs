using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Carly.Migrations
{
    public partial class EditedPackagesEntity2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverageStartDate",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "CoverageStopDate",
                table: "Packages");

            migrationBuilder.AddColumn<int>(
                name: "CoveragePeriod",
                table: "Packages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OwnerEmail",
                table: "Packages",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Packages",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OwnerPhoneNo",
                table: "Packages",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoveragePeriod",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "OwnerEmail",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "OwnerPhoneNo",
                table: "Packages");

            migrationBuilder.AddColumn<DateTime>(
                name: "CoverageStartDate",
                table: "Packages",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CoverageStopDate",
                table: "Packages",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
