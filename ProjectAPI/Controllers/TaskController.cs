using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Context;
using ProjectAPI.Models;
using System.Threading.Tasks;
using Task = ProjectAPI.Models.Task;

namespace ProjectAPI.Controllers
{
	[Route("api/task")]
	[ApiController]
	public class TaskController : ControllerBase
	{

		private readonly ProjectDbContext _authContext;
		public TaskController(ProjectDbContext projectDbContext)
		{
			_authContext = projectDbContext;
		}

		[HttpPost]
		public async Task<IActionResult> AddTask([FromBody] Task task)
		{

			await _authContext.Tasks.AddAsync(task);
			await _authContext.SaveChangesAsync();

			return Ok(new { taskId = task.TaskId, Message = "Task Added!" });
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Task>>> GetAllTasks([FromQuery] Guid taskboardId)

		{
			var tasks = await _authContext.Tasks
				.Where(t => t.TaskboardId == taskboardId) 
				.ToListAsync();

			if (!tasks.Any())
			{
				return NotFound(new { Message = "No tasks found for the specified Taskboard." });
			}

			return Ok(tasks);
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateTask(Guid id, [FromBody] Task updatedTask)
		{

			var existingTask = await _authContext.Tasks.FindAsync(id);

			if (existingTask == null)
			{
				return NotFound();
			}

			existingTask.Title = updatedTask.Title;
			existingTask.Status = updatedTask.Status;
			existingTask.Done = updatedTask.Done;
			existingTask.DueDate = updatedTask.DueDate;
			existingTask.Description = updatedTask.Description;
			existingTask.Priority = updatedTask.Priority;
			existingTask.LastEditedDate = updatedTask.LastEditedDate;
			existingTask.LastEditUserId = updatedTask.LastEditUserId;

			_authContext.Tasks.Update(existingTask);
			await _authContext.SaveChangesAsync();

			return Ok(new { taskId = updatedTask.TaskId, Message = "Task Updated!" });
		}

		[HttpDelete]
		[Route("{id:guid}")]
		public async Task<IActionResult> DeleteTask([FromRoute] Guid id)
		{
			var task = await _authContext.Tasks.FindAsync(id);

			if (task == null)
			{
				return NotFound();
			}

			_authContext.Tasks.Remove(task);

			await _authContext.SaveChangesAsync();

			return Ok(new {Message = "Task Deleted"});
		}

	}
}
