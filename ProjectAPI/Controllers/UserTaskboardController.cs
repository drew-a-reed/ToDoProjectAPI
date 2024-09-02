using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.Context;
using ProjectAPI.Models;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ProjectAPI.Controllers
{
	[Route("api/usertaskboard")]
	[ApiController]
	public class UserTaskboardController : ControllerBase
	{
		private readonly ProjectDbContext _authContext;

		public UserTaskboardController(ProjectDbContext projectDbContext)
		{
			_authContext = projectDbContext;
		}

		[HttpPost]
		public async Task<IActionResult> AddUserTaskboards([FromBody] UserTaskboard userTaskboard)
		{
			if (userTaskboard.UserId == Guid.Empty || userTaskboard.TaskboardId == Guid.Empty)
			{
				return BadRequest(new { message = "User ID and Taskboard ID cannot be empty." });
			}

			var existingUserTaskboard = await _authContext.UserTaskboards
				.FirstOrDefaultAsync(utb => utb.UserId == userTaskboard.UserId && utb.TaskboardId == userTaskboard.TaskboardId);

			if (existingUserTaskboard != null)
			{
				return NoContent();
			}

			_authContext.UserTaskboards.Add(userTaskboard);
			await _authContext.SaveChangesAsync();

			return Ok(new { message = "UserTaskboard added successfully." });
		}

		[HttpGet("{userId}")]
		public async Task<IActionResult> GetUserTaskboards(Guid userId)
		{
			if (userId == Guid.Empty)
			{
				return BadRequest(new { message = "User ID cannot be empty." });
			}

			var userTaskboards = await _authContext.UserTaskboards
				.Where(utb => utb.UserId == userId)
				.Join(
					_authContext.Taskboards,
					userTaskboard => userTaskboard.TaskboardId,
					taskboard => taskboard.TaskboardId,
					(userTaskboard, taskboard) => new
					{
						userTaskboard.UserId,
						userTaskboard.TaskboardId,
						TaskboardName = taskboard.TaskboardName,
						Role = userTaskboard.Role
					})
				.ToListAsync();

			if (userTaskboards == null || !userTaskboards.Any())
			{
				return NotFound(new { message = "No taskboards found for the specified user." });
			}

			return Ok(userTaskboards);
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
