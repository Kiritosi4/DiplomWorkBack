using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomWork.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class init456 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Targets_OwnerId",
                table: "Targets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Profits_OwnerId",
                table: "Profits",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfitCategories_OwnerID",
                table: "ProfitCategories",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_OwnerId",
                table: "Expenses",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_OwnerID",
                table: "ExpenseCategories",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_OwnerId",
                table: "Budgets",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Users_OwnerId",
                table: "Budgets",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseCategories_Users_OwnerID",
                table: "ExpenseCategories",
                column: "OwnerID",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Users_OwnerId",
                table: "Expenses",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfitCategories_Users_OwnerID",
                table: "ProfitCategories",
                column: "OwnerID",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Profits_Users_OwnerId",
                table: "Profits",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Targets_Users_OwnerId",
                table: "Targets",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Users_OwnerId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseCategories_Users_OwnerID",
                table: "ExpenseCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Users_OwnerId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfitCategories_Users_OwnerID",
                table: "ProfitCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Profits_Users_OwnerId",
                table: "Profits");

            migrationBuilder.DropForeignKey(
                name: "FK_Targets_Users_OwnerId",
                table: "Targets");

            migrationBuilder.DropIndex(
                name: "IX_Targets_OwnerId",
                table: "Targets");

            migrationBuilder.DropIndex(
                name: "IX_Profits_OwnerId",
                table: "Profits");

            migrationBuilder.DropIndex(
                name: "IX_ProfitCategories_OwnerID",
                table: "ProfitCategories");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_OwnerId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCategories_OwnerID",
                table: "ExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_OwnerId",
                table: "Budgets");
        }
    }
}
