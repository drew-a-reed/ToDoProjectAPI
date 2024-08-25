using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Context;
using ProjectAPI.Models;
using System;
using System.Threading.Tasks;

namespace ProjectAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserTaskboardController : ControllerBase
	{
		private readonly ProjectDbContext _authContext;

		public UserTaskboardController(ProjectDbContext projectDbContext)
		{
			_authContext = projectDbContext;
		}

		[HttpPost]
		[Route("user_taskboards")]
		public async Task<IActionResult> AddUserTaskboards([FromBody] Guid userId, Guid taskboardId)
		{
			if (userId == Guid.Empty || taskboardId == Guid.Empty)
			{
				return BadRequest("User ID and Taskboard ID cannot be empty.");
			}

			var userTaskboard = new UserTaskboard
			{
				UserId = userId,
				TaskboardId = taskboardId
			};
			_authContext.UserTaskboards.Add(userTaskboard);

			await _authContext.SaveChangesAsync();

			return Ok("UserTaskboard added successfully.");
		}

		[HttpDelete("user/{userId}/taskboard/{taskboardId}")]
		public async Task<IActionResult> DeleteUserFromTaskboard(Guid userId, Guid taskboardId)
		{
			var userTaskboard = await _authContext.UserTaskboards
				.FirstOrDefaultAsync(utb => utb.UserId == userId && utb.TaskboardId == taskboardId);

			if (userTaskboard == null)
			{
				return NotFound("User-taskboard association not found.");
			}

			_authContext.UserTaskboards.Remove(userTaskboard);
			await _authContext.SaveChangesAsync();

			return Ok(new { Message = "User removed from taskboard successfully." });
		}


	}
}
