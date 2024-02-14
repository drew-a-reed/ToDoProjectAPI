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
using ProjectAPI.Context;
using ProjectAPI.Helpers;
using ProjectAPI.Models;
using ProjectAPI.Models.Dto;

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
			var key = Encoding.ASCII.GetBytes("Fmxg2JrpJAfcOmpiMSIipCGkaoYOptOmwO5EueHU1z8Y8ewQw4lyuhChI65nyxRubUC6ameUiDUdl2SUAFeq3ia25z4F95eBc0RSlYEDAX8OAzjuCNyc5pjtKHjHZDWr");
			var identity = new ClaimsIdentity(new Claim[]
			{
				new Claim(ClaimTypes.Role, userObj.Role),
				new Claim(ClaimTypes.Name, $"{userObj.Username}")
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

		[Authorize]
		[HttpGet]
		public async Task<ActionResult<User>> GetAllUsers()
		{
			return Ok(await _authContext.Users.ToListAsync());
		}

		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh(TokenApiDto tokenApiDto)
		{
			if (tokenApiDto is null)
				return BadRequest("Invalid Client Request");

			string accessToken = tokenApiDto.AccessToken;
			string refreshToken = tokenApiDto.RefreshToken;
			var principal = GetPrincipalFromExpiredToken(accessToken);
			var username = principal.Identity.Name;
			var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Username == username);
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

	}
}
