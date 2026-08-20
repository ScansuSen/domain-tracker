using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DomainTracker.Core.Constants;
using DomainTracker.Core.Results;
using DomainTracker.Core.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DomainTracker.API.Middleware
{
    public class JwtAuthenticationMiddleware
    {
        private const string BearerPrefix = "Bearer ";

        private readonly RequestDelegate _next;

        public JwtAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOptions<JwtSettings> jwtSettingsOptions)
        {
            var principal = TryValidateToken(context, jwtSettingsOptions.Value);
            if (principal is not null)
            {
                context.User = principal;
            }

            if (principal is null && RequiresAuthentication(context))
            {
                await WriteUnauthorizedAsync(context);
                return;
            }

            await _next(context);
        }

        private static bool RequiresAuthentication(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            {
                return false;
            }

            return endpoint?.Metadata.GetMetadata<IAuthorizeData>() is not null;
        }

        private static ClaimsPrincipal? TryValidateToken(HttpContext context, JwtSettings settings)
        {
            var header = context.Request.Headers["Authorization"].FirstOrDefault();
            if (header is null || !header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var token = header[BearerPrefix.Length..].Trim();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
            };

            try
            {
                return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                return null;
            }
        }

        private static async Task WriteUnauthorizedAsync(HttpContext context)
        {
            context.Response.StatusCode = HttpStatusCodes.Unauthorized;
            context.Response.ContentType = "application/json";
            var response = new ErrorResult(HttpStatusCodes.Unauthorized, Messages.AuthenticationRequired);
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
