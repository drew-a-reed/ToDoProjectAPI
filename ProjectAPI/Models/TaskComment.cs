using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.Models
{
	public class TaskComment
	{
		[Key]
		public int CommentId { get; set; }

		[ForeignKey("User")]
		public Guid UserId { get; set; }

		public string UserName { get; set; }

		[ForeignKey("Task")]
		public Guid TaskId { get; set; }

		public string Comment {  get; set; }

		public DateTime CreatedDate { get; set; } = DateTime.Now;

		public DateTime? LastModifiedDate { get; set;}

	}
}
