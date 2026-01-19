using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LevelingSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompletedQuestTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedQuestTitles",
                table: "JournalEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "CompletedQuestTitles",
                table: "JournalEntries",
                type: "jsonb",
                nullable: false);
        }
    }
}
