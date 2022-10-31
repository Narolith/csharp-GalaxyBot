#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GalaxyBot.Migrations
{
    public partial class addClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Class",
                table: "GroupUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Class",
                table: "GroupUsers");
        }
    }
}
