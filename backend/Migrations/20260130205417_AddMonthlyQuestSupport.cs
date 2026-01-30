using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LevelingSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyQuestSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonthlyDay",
                table: "QuestDefinitions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyDay",
                table: "QuestDefinitions");
        }
    }
}
