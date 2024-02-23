using Microsoft.AspNetCore.Mvc;
using ProjectAPI.Context;
using ProjectAPI.Models;

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
		public async Task<IActionResult> AddUserTask([FromBody] UserTask userTask)
		{
			await _authContext.UserTasks.AddAsync(userTask);
			await _authContext.SaveChangesAsync();

			return Ok(new { Message = "User Task Added!" });

		}

	}
}
