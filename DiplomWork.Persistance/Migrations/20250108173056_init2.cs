using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomWork.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TargetAmount",
                table: "Targets",
                newName: "Limit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Limit",
                table: "Targets",
                newName: "TargetAmount");
        }
    }
}
