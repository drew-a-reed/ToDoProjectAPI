using Microsoft.EntityFrameworkCore;
using ProjectAPI.Models;
using Task = ProjectAPI.Models.Task;

namespace ProjectAPI.Context
{
	public class ProjectDbContext: DbContext
	{

		public ProjectDbContext(DbContextOptions<ProjectDbContext> options):base(options)
		{
			
		}

		public DbSet<User> Users { get; set; }
		public DbSet<Task> Tasks { get; set; }
		public DbSet<UserTask> UserTasks { get; set; }
		public DbSet<TaskComment> TaskComments { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>().ToTable("users");
			modelBuilder.Entity<Task>().ToTable("tasks");
			modelBuilder.Entity<UserTask>().ToTable("user-tasks");
			modelBuilder.Entity<TaskComment>().ToTable("task-comments");
		}

	}
}
