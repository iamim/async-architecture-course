using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsyncArch.Services.Tasks.Migrations
{
    public partial class renamedescriptiontotitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "description",
                table: "tasks",
                newName: "title");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "title",
                table: "tasks",
                newName: "description");
        }
    }
}
