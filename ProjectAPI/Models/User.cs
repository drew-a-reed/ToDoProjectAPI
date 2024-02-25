﻿using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.Models
{
	public class User
	{

		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string Token { get; set; }
		public string Role { get; set; }
		public string RefreshToken { get; set; }
		public DateTime RefreshTokenExpireTime { get; set; }
		public string ResetPasswordToken { get; set; }
		public DateTime ResetPasswordTokenExpireTime { get; set;}


	}
}
