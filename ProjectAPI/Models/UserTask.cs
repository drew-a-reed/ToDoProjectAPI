using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectAPI.Models
{
	public class UserTask
	{
		[Key]
		public int UserTaskId { get; set; }

		[ForeignKey("User")]
		public Guid UserId { get; set; }
		public User User { get; set; }

		[ForeignKey("Task")]
		public Guid TaskId { get; set; }
		public Task Task { get; set; }
	}
}