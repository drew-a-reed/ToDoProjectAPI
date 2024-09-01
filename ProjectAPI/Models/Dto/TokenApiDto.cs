namespace ProjectAPI.Models.Dto
{
	public class TokenApiDto
	{

		public string AccessToken { get; set; } = string.Empty;

		public string RefreshToken { get; set; } = string.Empty;

		public Guid UserId { get; set; }
	}
}
