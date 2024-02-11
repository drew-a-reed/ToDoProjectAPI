using Microsoft.EntityFrameworkCore;
using ProjectAPI.Models;

namespace ProjectAPI.Context
{
	public class ProjectDbContext: DbContext
	{

		public ProjectDbContext(DbContextOptions<ProjectDbContext> options):base(options)
		{
			
		}

		public DbSet<User> Users { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>().ToTable("users");
		}

	}
}
