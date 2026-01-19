using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LevelingSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2_StreakAndFatigue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoAssign",
                table: "QuestDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DeadlineHours",
                table: "QuestDefinitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurrenceType",
                table: "QuestDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FatigueLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    FatigueLevel = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    QuestsCompleted = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    QuestsAssigned = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FatigueLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FatigueLogs_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Streaks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    StreakType = table.Column<string>(type: "text", nullable: false),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LongestStreak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastCompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Streaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Streaks_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FatigueLogs_CharacterId_Date",
                table: "FatigueLogs",
                columns: new[] { "CharacterId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Streaks_CharacterId_StreakType",
                table: "Streaks",
                columns: new[] { "CharacterId", "StreakType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FatigueLogs");

            migrationBuilder.DropTable(
                name: "Streaks");

            migrationBuilder.DropColumn(
                name: "AutoAssign",
                table: "QuestDefinitions");

            migrationBuilder.DropColumn(
                name: "DeadlineHours",
                table: "QuestDefinitions");

            migrationBuilder.DropColumn(
                name: "RecurrenceType",
                table: "QuestDefinitions");
        }
    }
}
