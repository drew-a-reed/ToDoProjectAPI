using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectAPI.Migrations
{
    /// <inheritdoc />
    public partial class addpriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaskboardID",
                table: "tasks",
                newName: "TaskboardId");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "tasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "tasks");

            migrationBuilder.RenameColumn(
                name: "TaskboardId",
                table: "tasks",
                newName: "TaskboardID");
        }
    }
}
