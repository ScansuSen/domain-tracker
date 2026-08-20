using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainTracker.Business.Abstract;
using DomainTracker.Core.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DomainTracker.Business.Concrete
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _settings;

        public JwtTokenGenerator(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public (string Token, DateTime ExpiresAtUtc) GenerateToken(int userId, string username)
        {
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

            var userIdText = userId.ToString(CultureInfo.InvariantCulture);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userIdText),
                new Claim(ClaimTypes.NameIdentifier, userIdText),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            var handler = new JwtSecurityTokenHandler();
            return (handler.WriteToken(token), expiresAtUtc);
        }
    }
}
