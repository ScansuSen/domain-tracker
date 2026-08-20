using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainTracker.Business.Concrete;
using DomainTracker.Core.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace DomainTracker.Tests.Business
{
    public class JwtTokenGeneratorTests
    {
        private static readonly JwtSettings Settings = new()
        {
            Key = "unit-test-signing-key-needs-to-be-long-enough-for-hmac-sha256",
            Issuer = "DomainTracker.Tests",
            Audience = "DomainTracker.Tests.Client",
            ExpiryMinutes = 30,
        };

        private static JwtTokenGenerator CreateGenerator() => new(Options.Create(Settings));

        [Fact]
        public void GenerateToken_ProducesTokenValidWithTheSameSettings()
        {
            var generator = CreateGenerator();

            var (token, expiresAtUtc) = generator.GenerateToken(userId: 7, username: "alice");

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Settings.Issuer,
                ValidateAudience = true,
                ValidAudience = Settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Key)),
                ValidateLifetime = true,
            };

            var principal = handler.ValidateToken(token, validationParameters, out _);

            Assert.Equal("7", principal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.Equal("alice", principal.FindFirstValue(ClaimTypes.Name));
            Assert.True(expiresAtUtc > DateTime.UtcNow);
            Assert.True(expiresAtUtc <= DateTime.UtcNow.AddMinutes(Settings.ExpiryMinutes).AddSeconds(5));
        }

        [Fact]
        public void GenerateToken_WithWrongSigningKey_FailsValidation()
        {
            var generator = CreateGenerator();
            var (token, _) = generator.GenerateToken(userId: 1, username: "someone");

            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Settings.Issuer,
                ValidateAudience = true,
                ValidAudience = Settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a-completely-different-signing-key-value-here")),
                ValidateLifetime = true,
            };

            Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(
                () => handler.ValidateToken(token, validationParameters, out _));
        }
    }
}
