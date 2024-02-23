using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Context;
using ProjectAPI.Models;
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

			return Ok(new { Message = "Task Added!" });
		}

		[HttpGet]
		public async Task<ActionResult<Task>> GetAllTasks()
		{
			return Ok(await _authContext.Tasks.ToListAsync());
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
