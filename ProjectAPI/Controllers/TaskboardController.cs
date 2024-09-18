using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using ProjectAPI.Context;
using ProjectAPI.Helpers;
using ProjectAPI.Models;
using ProjectAPI.Models.Dto;
using IEmailService = ProjectAPI.UtilityService.IEmailService;

namespace ProjectAPI.Controllers
{
	[Route("api/taskboard")]
	[ApiController]
	public class TaskboardController : ControllerBase
	{
		private readonly ProjectDbContext _authContext;
		private readonly IConfiguration _configuration;
		private readonly IEmailService _emailService;
		public TaskboardController(ProjectDbContext projectDbContext, IConfiguration configuration, IEmailService emailService)
		{
			_authContext = projectDbContext;
			_configuration = configuration;
			_emailService = emailService;
		}

		[HttpPost("authenticate")]
		public async Task<ActionResult> Authenticate([FromBody] Taskboard taskboardObj)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var taskboard = await _authContext.Taskboards
				.FirstOrDefaultAsync(x => x.TaskboardName == taskboardObj.TaskboardName);

			if (taskboard == null || !PasswordHasher.VerifyPassword(taskboardObj.TaskboardPassword, taskboard.TaskboardPassword))
			{
				return NotFound(new { Message = "Invalid taskboard name or password" });
			}

			return Ok(new
			{
				TaskboardId = taskboard.TaskboardId,
				Message = "Authentication successful"
			});
		}

		[HttpPost("register")]
		public async Task<ActionResult> RegisterTaskboard([FromBody] Taskboard taskboardObj)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var passStrengthMsg = CheckPasswordStrength(taskboardObj.TaskboardPassword);
			if (!string.IsNullOrEmpty(passStrengthMsg))
				return BadRequest(new { Message = passStrengthMsg });

			var existingTaskboards = await _authContext.Taskboards.FirstOrDefaultAsync(et => et.TaskboardName == taskboardObj.TaskboardName);

			if (existingTaskboards != null)
				return Conflict("Must choose unique taskboard name");

			var taskboard = new Taskboard
			{
				TaskboardId = Guid.NewGuid(),
				TaskboardName = taskboardObj.TaskboardName,
				TaskboardPassword = PasswordHasher.HashPassword(taskboardObj.TaskboardPassword),
				DateTaskboardCreated = DateTime.UtcNow,
			};

			await _authContext.Taskboards.AddAsync(taskboard);
			await _authContext.SaveChangesAsync();

			return Ok(new { TaskboardId = taskboard.TaskboardId, Message = "Taskboard Registered!" });
		}

		[HttpGet("taskboards")]
		public async Task<ActionResult<Taskboard>> GetAllTaskboards()
		{
			return Ok(await _authContext.Taskboards.ToListAsync());
		}

		[HttpDelete]
		[Route("{id:guid}")]
		public async Task<IActionResult> DeleteTaskboard([FromRoute] Guid id)
		{
			var taskboard = await _authContext.Taskboards.FindAsync(id);

			if (taskboard == null)
			{
				return NotFound();
			}

			_authContext.Taskboards.Remove(taskboard);

			await _authContext.SaveChangesAsync();

			return Ok(new { Message = "Taskboard Deleted!" });
		}

		private string CheckPasswordStrength(string password)
		{
			StringBuilder sb = new StringBuilder();

			if (password.Length < 8)
			{
				sb.Append("Minimum password length should be 8" + Environment.NewLine);
			}

			if (!(Regex.IsMatch(password, "[a-z]")
				  && Regex.IsMatch(password, "[A-Z]")
				  && Regex.IsMatch(password, "[0-9]")))
			{
				sb.Append("Password should contain a-z AND A-Z AND 0-9" + Environment.NewLine);
			}

			if (!Regex.IsMatch(password, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\], {, }, ?, :, ;, |, ', \\, >, /, ~, `, -,+]"))
			{
				sb.Append("Password must contain a special character");
			}

			return sb.ToString();
		}


	}
}
