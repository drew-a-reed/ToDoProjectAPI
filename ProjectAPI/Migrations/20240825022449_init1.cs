using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectAPI.Migrations
{
    /// <inheritdoc />
    public partial class init1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user-tasks",
                table: "user-tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_task-comments",
                table: "task-comments");

            migrationBuilder.RenameTable(
                name: "user-tasks",
                newName: "user_tasks");

            migrationBuilder.RenameTable(
                name: "task-comments",
                newName: "task_comments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_tasks",
                table: "user_tasks",
                column: "UserTaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_task_comments",
                table: "task_comments",
                column: "CommentId");

            migrationBuilder.CreateTable(
                name: "taskboards",
                columns: table => new
                {
                    TaskboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskboardName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaskboardPassword = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_taskboards", x => x.TaskboardId);
                });

            migrationBuilder.CreateTable(
                name: "user_taskboards",
                columns: table => new
                {
                    UserTaskboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_taskboards", x => x.UserTaskboardId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "taskboards");

            migrationBuilder.DropTable(
                name: "user_taskboards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_tasks",
                table: "user_tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_task_comments",
                table: "task_comments");

            migrationBuilder.RenameTable(
                name: "user_tasks",
                newName: "user-tasks");

            migrationBuilder.RenameTable(
                name: "task_comments",
                newName: "task-comments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user-tasks",
                table: "user-tasks",
                column: "UserTaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_task-comments",
                table: "task-comments",
                column: "CommentId");
        }
    }
}
