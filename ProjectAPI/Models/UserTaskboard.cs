using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.Models
{
	public class UserTaskboard
	{
		[Key]
		public Guid UserTaskboardId { get; set; } = Guid.NewGuid();

		[ForeignKey("User")]
		public Guid UserId { get; set; }

		[ForeignKey("Taskboard")]
		public Guid TaskboardId { get; set; }

		public string Role {  get; set; }

	}
}
