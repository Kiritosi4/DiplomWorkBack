using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomWork.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class init123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profits_Targets_TargetId",
                table: "Profits");

            migrationBuilder.DropIndex(
                name: "IX_Profits_TargetId",
                table: "Profits");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "Profits");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Targets",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "Closed",
                table: "Targets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "EndPeriod",
                table: "Budgets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "StartPeriod",
                table: "Budgets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Targets");

            migrationBuilder.DropColumn(
                name: "Closed",
                table: "Targets");

            migrationBuilder.DropColumn(
                name: "EndPeriod",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "StartPeriod",
                table: "Budgets");

            migrationBuilder.AddColumn<Guid>(
                name: "TargetId",
                table: "Profits",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profits_TargetId",
                table: "Profits",
                column: "TargetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profits_Targets_TargetId",
                table: "Profits",
                column: "TargetId",
                principalTable: "Targets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
