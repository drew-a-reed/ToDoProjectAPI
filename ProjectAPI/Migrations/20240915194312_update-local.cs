using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectAPI.Migrations
{
    /// <inheritdoc />
    public partial class updatelocal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "taskboards",
                newName: "DateTaskboardCreated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTaskboardCreated",
                table: "taskboards",
                newName: "DateCreated");
        }
    }
}
