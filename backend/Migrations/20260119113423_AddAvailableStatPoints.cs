using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LevelingSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailableStatPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableStatPoints",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableStatPoints",
                table: "Characters");
        }
    }
}
