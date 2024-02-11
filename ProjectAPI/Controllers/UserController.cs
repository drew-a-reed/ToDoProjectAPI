using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectAPI.Context;
using ProjectAPI.Helpers;
using ProjectAPI.Models;

namespace ProjectAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly ProjectDbContext _authContext;
		public UserController(ProjectDbContext projectDbContext)
		{
			_authContext = projectDbContext;
		}

		[HttpPost("authenticate")]
		public async Task<ActionResult> Authenticate([FromBody] User userObj)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Username == userObj.Username);

			if (user == null || !PasswordHasher.VerifyPassword(userObj.Password, user.Password))
			{
				return NotFound(new { Message = "Invalid username or password" });
			}

			user.Token = CreateJwt(user);

			return Ok(new
			{
				Token = user.Token,
				Message = "Login Success!"
			});
		}

		[HttpPost("register")]
		public async Task<ActionResult> RegisterUser([FromBody] User userObj)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (await CheckUsernameExistAsync(userObj.Username))
				return BadRequest(new { Message = "Username Already Exists" });

			if (await CheckEmailExistAsync(userObj.Email))
				return BadRequest(new { Message = "Email Already Exists" });

			var passStrengthMsg = CheckPasswordStrength(userObj.Password);
			if (!string.IsNullOrEmpty(passStrengthMsg))
				return BadRequest(new { Message = passStrengthMsg });

			userObj.Password = PasswordHasher.HashPassword(userObj.Password);
			userObj.Role = "User";
			userObj.Token = "";

			await _authContext.Users.AddAsync(userObj);
			await _authContext.SaveChangesAsync();

			return Ok(new { Message = "User Registered!" });
		}

		private async Task<bool> CheckUsernameExistAsync(string username)
		{
			return await _authContext.Users.AnyAsync(x => x.Username == username);
		}

		private async Task<bool> CheckEmailExistAsync(string email)
		{
			return await _authContext.Users.AnyAsync(x => x.Email == email);
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

		private string CreateJwt(User userObj)
		{
			var jwtHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes("310095D274CC9C1949800A380C43359E19F0E7325819586B3F624E09C6D3FA803A641676478277AD7183FAB48691691A28900FFB05592DFF337ACFA2262328BF");
			var identity = new ClaimsIdentity(new Claim[]
			{
				new Claim(ClaimTypes.Role, userObj.Role),
				new Claim(ClaimTypes.Name, $"{userObj.FirstName} {userObj.LastName}")
			});
			var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = identity,
				Expires = DateTime.Now.AddDays(1),
				SigningCredentials = credentials
			};

			var token = jwtHandler.CreateToken(tokenDescriptor);

			return jwtHandler.WriteToken(token);
		}

		[HttpGet]
		public async Task<ActionResult<User>> GetAllUsers()
		{
			return Ok(await _authContext.Users.ToListAsync());
		}

	}
}
