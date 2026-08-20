namespace DomainTracker.DTOs.Auth
{
    public class AuthResponseDto
    {
        public AuthResponseDto(string token, string username, DateTime expiresAtUtc)
        {
            Token = token;
            Username = username;
            ExpiresAtUtc = expiresAtUtc;
        }

        public string Token { get; }

        public string Username { get; }

        public DateTime ExpiresAtUtc { get; }
    }
}
