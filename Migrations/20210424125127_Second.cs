using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApp.Migrations
{
    public partial class Second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrentSemester",
                table: "Students",
                newName: "CurrentSemester");
            }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
