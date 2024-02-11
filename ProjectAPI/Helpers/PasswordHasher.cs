using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ProjectAPI.Helpers
{
	public class PasswordHasher
	{
		private const int SaltSize = 128 / 8;
		private const int HashSize = 256 / 8;
		private const int Iterations = 10000;

		public static string HashPassword(string password)
		{
			byte[] salt = GenerateSalt();

			string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: password,
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA256,
				iterationCount: Iterations,
				numBytesRequested: HashSize
			));

			return Convert.ToBase64String(salt) + "|" + hashed;
		}

		public static bool VerifyPassword(string password, string storedPasswordHash)
		{
			string[] parts = storedPasswordHash.Split('|');
			if (parts.Length != 2)
			{
				return false;
			}

			byte[] salt = Convert.FromBase64String(parts[0]);
			string storedHashedPassword = parts[1];

			string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: password,
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA256,
				iterationCount: Iterations,
				numBytesRequested: HashSize
			));

			return storedHashedPassword == hashed;
		}

		private static byte[] GenerateSalt()
		{
			byte[] salt = new byte[SaltSize];
			using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(salt);
			}
			return salt;
		}
	}
}
