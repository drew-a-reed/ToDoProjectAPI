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
		public async Task<IActionResult> AddUserToTaskboard([FromBody] UserTaskboard userTaskboard)
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

			return Ok(new { message = "User added to taskboard successfully." });
		}

		[HttpGet("{userId}/taskboards")]
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
						taskboard.TaskboardName
					})
				.ToListAsync();

			if (userTaskboards == null || !userTaskboards.Any())
			{
				return NotFound(new { message = "No taskboards found for the specified user." });
			}

			return Ok(userTaskboards);
		}

		[HttpGet("{taskboardId}/users")]
		public async Task<IActionResult> GetTaskboardUsers(Guid taskboardId)
		{
			if (taskboardId == Guid.Empty)
			{
				return BadRequest(new { message = "Taskboard ID cannot be empty." });
			}

			var taskboardUsers = await _authContext.UserTaskboards
				.Where(tbu => tbu.TaskboardId == taskboardId)
				.Join(
					_authContext.Users,
					userTaskboard => userTaskboard.UserId,
					user => user.UserId,
					(userTaskboard, user) => new
					{
						user.UserId,
						user.FirstName,
						user.LastName,
						user.Email,
						userTaskboard.Role
					})
				.ToListAsync();

			if (taskboardUsers == null || !taskboardUsers.Any())
			{
				return NotFound(new { message = "No users found for the specified taskboard." });
			}

			return Ok(taskboardUsers);
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
