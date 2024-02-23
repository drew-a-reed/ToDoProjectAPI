using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.Models
{
	public class Task
	{
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid(); // Use Guid as the type for ID


		public string Description { get; set; }
		public string Status { get; set; } = "To Do";
		public bool Done { get; set; } = false;
	}
}