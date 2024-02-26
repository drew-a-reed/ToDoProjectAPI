using Microsoft.AspNetCore.Mvc;
using ProjectAPI.Context;
using ProjectAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserTaskController : ControllerBase
	{
		private readonly ProjectDbContext _authContext;
		public UserTaskController(ProjectDbContext projectDbContext)
		{
			_authContext = projectDbContext;
		}

		[HttpPost]
		[Route("user-tasks")]
		public async Task<IActionResult> AddUserTasks([FromBody] List<UserTask> userTasks)
		{
			if (userTasks == null || !userTasks.Any())
			{
				return BadRequest("User tasks data is missing.");
			}

			foreach (var userTask in userTasks)
			{
				await _authContext.UserTasks.AddAsync(userTask);
			}

			await _authContext.SaveChangesAsync();

			return Ok(new { Message = "User Tasks Added!" });
		}


		[HttpGet]
		[Route("user-tasks")]
		public IActionResult GetAllUserTasks()
		{
			var userTasks = _authContext.UserTasks.ToList();
			return Ok(userTasks);
		}

		[HttpGet]
		[Route("users/{taskId}/tasks")]
		public IActionResult GetAllUsersByTaskId(Guid taskId)
		{
			var users = _authContext.UserTasks.Where(ut => ut.TaskId == taskId).Select(ut => ut.UserId).ToList();
			return Ok(users);
		}

		[HttpGet]
		[Route("tasks/{userId}/users")]
		public IActionResult GetAllTasksByUserId(Guid userId)
		{
			var tasks = _authContext.UserTasks.Where(ut => ut.UserId == userId).Select(ut => ut.TaskId).ToList();
			return Ok(tasks);
		}

		[HttpDelete]
		[Route("users/{userId}/tasks/{taskId}")]
		public async Task<IActionResult> DeleteTaskFromUser(Guid userId, Guid taskId)
		{
			var userTask = _authContext.UserTasks.FirstOrDefault(ut => ut.UserId == userId && ut.TaskId == taskId);

			if (userTask == null)
			{
				return NotFound();
			}

			_authContext.UserTasks.Remove(userTask);
			await _authContext.SaveChangesAsync();

			return Ok(new { Message = "Task deleted from user." });
		}

		[HttpDelete]
		[Route("tasks/{taskId}/users/{userId}")]
		public async Task<IActionResult> DeleteUserFromTask(Guid taskId, Guid userId)
		{
			var userTask = _authContext.UserTasks.FirstOrDefault(ut => ut.TaskId == taskId && ut.UserId == userId);

			if (userTask == null)
			{
				return NotFound();
			}

			_authContext.UserTasks.Remove(userTask);
			await _authContext.SaveChangesAsync();

			return Ok(new { Message = "User deleted from task." });
		}
	}
}
