using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsyncArch.Services.Tasks.Migrations
{
    public partial class addjiraidtotaskstable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "jira_id",
                table: "tasks",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "jira_id",
                table: "tasks");
        }
    }
}
