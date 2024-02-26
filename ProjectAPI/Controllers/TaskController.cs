using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Context;
using ProjectAPI.Models;
using System.Threading.Tasks;
using Task = ProjectAPI.Models.Task;

namespace ProjectAPI.Controllers
{
	[Route("api/[controller]")]
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

			return Ok(new { TaskId = task.Id, Message = "Task Added!" });
		}

		[HttpGet]
		public async Task<ActionResult<Task>> GetAllTasks()
		{
			return Ok(await _authContext.Tasks.ToListAsync());
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> UpdateTask(Guid id, [FromBody] Task updatedTask)
		{


			var existingTask = await _authContext.Tasks.FindAsync(id);

			if (existingTask == null)
			{
				return NotFound();
			}

			existingTask.Description = updatedTask.Description;
			existingTask.Status = updatedTask.Status;
			existingTask.Done = updatedTask.Done;

			_authContext.Tasks.Update(existingTask);
			await _authContext.SaveChangesAsync();

			return Ok(new { TaskId = updatedTask.Id, Message = "Task Updated!" });
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

			return Ok();
		}

	}
}
