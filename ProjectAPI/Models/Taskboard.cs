using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.Models
{
	public class Taskboard
	{
		[Key]
		public Guid TaskboardId { get; set; } = Guid.NewGuid(); 

		public string TaskboardName { get; set; }

		public string TaskboardPassword { get; set; }

		public DateTime? DateTaskboardCreated { get; set; }
	}
}