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
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly ProjectDbContext _authContext;
		private readonly IConfiguration _configuration;
		private readonly IEmailService _emailService;
		public UserController(ProjectDbContext projectDbContext, IConfiguration configuration, IEmailService emailService)
		{
			_authContext = projectDbContext;
			_configuration = configuration;
			_emailService = emailService;
		}

		[HttpPost("authenticate")]
		public async Task<ActionResult> Authenticate([FromBody] User userObj)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == userObj.Email);

			if (user == null || !PasswordHasher.VerifyPassword(userObj.Password, user.Password))
			{
				return NotFound(new { Message = "Invalid email or password" });
			}

			user.Token = CreateJwt(user);
			var newAccessToken = user.Token;
			var newRefreshToken = CreateRefreshToken();
			user.RefreshToken = newRefreshToken;
			user.RefreshTokenExpireTime = DateTime.Now.AddDays(5);
			await _authContext.SaveChangesAsync();

			return Ok(new TokenApiDto()
			{
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken
			});
		}

		[HttpPost("register")]
		public async Task<ActionResult> RegisterUser([FromBody] User userObj)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

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
			var key = Encoding.ASCII.GetBytes("Fmxg2JrpJAfcOmpiMSIipCGkaoYOptOmwO5EueHU1z8Y8ewQw4lyuhChI65nyxRubUC6ameUiDUdl2SUAFeq3ia25z4F95eBc0RSlYEDAX8OAzjuCNyc5pjtKHjHZDWr");
			var identity = new ClaimsIdentity(new Claim[]
			{
				new Claim(ClaimTypes.Role, userObj.Role),
				new Claim(ClaimTypes.Name, $"{userObj.Email}")
			});
			var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = identity,
				Expires = DateTime.UtcNow.AddMinutes(15),
				SigningCredentials = credentials
			};

			var token = jwtHandler.CreateToken(tokenDescriptor);

			return jwtHandler.WriteToken(token);
		}

		private string CreateRefreshToken()
		{
			var tokenBytes = RandomNumberGenerator.GetBytes(64);
			var refreshToken = Convert.ToBase64String(tokenBytes);

			var tokenInUser = _authContext.Users.Any(a => a.RefreshToken == refreshToken);

			if (tokenInUser)
			{
				return CreateRefreshToken();
			}

			return refreshToken;
		}

		private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
		{
			var key = Encoding.ASCII.GetBytes("Fmxg2JrpJAfcOmpiMSIipCGkaoYOptOmwO5EueHU1z8Y8ewQw4lyuhChI65nyxRubUC6ameUiDUdl2SUAFeq3ia25z4F95eBc0RSlYEDAX8OAzjuCNyc5pjtKHjHZDWr");
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateLifetime = false
			};
			var tokenHandler = new JwtSecurityTokenHandler();
			SecurityToken securityToken;
			var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
			var jwtSecurityToken = securityToken as JwtSecurityToken;
			if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				throw new SecurityTokenException("This is invalid token");
			return principal;
		}

		[HttpGet("users")]
		public async Task<ActionResult<User>> GetAllUsers()
		{
			return Ok(await _authContext.Users.ToListAsync());
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<User>> GetUserById(Guid id)
		{
			var user = await _authContext.Users.FindAsync(id);

			if (user == null)
			{
				return NotFound();
			}

			return Ok(user);
		}

		[HttpPut("{userId}")]
		public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User updateUserDto)
		{
			var user = await _authContext.Users.FindAsync(userId);

			if (user == null)
			{
				return NotFound(new { Message = "User not found" });
			}

			user.Email = updateUserDto.Email ?? user.Email;
			user.FirstName = updateUserDto.FirstName ?? user.FirstName;
			user.LastName = updateUserDto.LastName ?? user.LastName;
			user.Role = updateUserDto.Role ?? user.Role;

			if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
			{
				user.Password = PasswordHasher.HashPassword(updateUserDto.Password);
			}

			_authContext.Users.Update(user);
			await _authContext.SaveChangesAsync();

			return Ok(new { Message = "User updated successfully", UserId = user.UserId });
		}

		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh(TokenApiDto tokenApiDto)
		{
			if (tokenApiDto is null)
				return BadRequest("Invalid Client Request");

			string accessToken = tokenApiDto.AccessToken;
			string refreshToken = tokenApiDto.RefreshToken;
			var principal = GetPrincipalFromExpiredToken(accessToken);
			var email = principal.Identity.Name;
			var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpireTime <= DateTime.Now)
				return BadRequest("Invalid Request");
			var newAccessToken = CreateJwt(user);
			var newRefreshToken = CreateRefreshToken();
			user.RefreshToken = newRefreshToken;
			await _authContext.SaveChangesAsync();

			return Ok(new TokenApiDto()
			{
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken
			});
		}

		[HttpPost("send-reset-email/{email}")]
		public async Task<IActionResult> SendEmail(string email)
		{
			var user = await _authContext.Users.FirstOrDefaultAsync(x => x.Email == email);
			if (user == null)
				return NotFound(new
				{
					StatusCode = 404,
					Message = "Email does not exist"
				});
			var tokenBytes = RandomNumberGenerator.GetBytes(64);
			var emailToken = Convert.ToBase64String(tokenBytes);
			user.ResetPasswordToken = emailToken;
			user.ResetPasswordTokenExpireTime = DateTime.Now.AddMinutes(15);
			string from = _configuration["EmailSettings:From"];
			var emailModel = new EmailModel(email, "Reset Password", EmailBody.EmailStringBody(email, emailToken));
			_emailService.SendEmail(emailModel);
			_authContext.Entry(user).State = EntityState.Modified;
			await _authContext.SaveChangesAsync();

			return Ok(new
			{
				StatusCode = 200,
				Message = "Email Sent!"
			});
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
		{
			var newToken = resetPasswordDto.EmailToken.Replace(" ", "+");
			var user = await _authContext.Users.AsNoTracking()
				.FirstOrDefaultAsync(x => x.Email == resetPasswordDto.Email);
			if (user == null)
				return NotFound(new
				{
					StatusCode = 404,
					Message = "User does not exist"
				});
			var tokenCode = user.ResetPasswordToken;
			DateTime emailTokenExpireTime = user.ResetPasswordTokenExpireTime;
			if (tokenCode != resetPasswordDto.EmailToken || emailTokenExpireTime < DateTime.Now)
				return BadRequest(new
				{
					StatusCode = 400,
					Message = "Reset link has expired."
				});
			user.Password = PasswordHasher.HashPassword(resetPasswordDto.NewPassword);
			_authContext.Entry(user).State = EntityState.Modified;
			await _authContext.SaveChangesAsync();
			return Ok(new
			{
				StatusCode = 200,
				Message = "Password has been reset"
			});
		}

	}
}
