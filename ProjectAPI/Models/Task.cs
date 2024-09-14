using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectAPI.Models
{
	public class Task
	{
		[Key]
		public Guid TaskId { get; set; } = Guid.NewGuid();
		public Guid? CreatedByUserId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Status { get; set; } = "To Do";
		public string Priority { get; set; }
		public bool Done { get; set; } = false;
		public DateTime AssignedDate { get; set; }
		public DateTime DueDate { get; set; }
		public DateTime? LastEditedDate { get; set; }
		public Guid? LastEditUserId { get; set; }

		[ForeignKey("Taskboard")]
		public Guid TaskboardId { get; set; }
	}
}