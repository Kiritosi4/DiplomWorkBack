using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomWork.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class init54 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profits_ProfitCategories_CategoryId",
                table: "Profits");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CategoryId",
                table: "Expenses",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_ExpenseCategories_CategoryId",
                table: "Expenses",
                column: "CategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Profits_ProfitCategories_CategoryId",
                table: "Profits",
                column: "CategoryId",
                principalTable: "ProfitCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_ExpenseCategories_CategoryId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Profits_ProfitCategories_CategoryId",
                table: "Profits");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_CategoryId",
                table: "Expenses");

            migrationBuilder.AddForeignKey(
                name: "FK_Profits_ProfitCategories_CategoryId",
                table: "Profits",
                column: "CategoryId",
                principalTable: "ProfitCategories",
                principalColumn: "Id");
        }
    }
}
