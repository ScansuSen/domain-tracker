using System.Security.Claims;
using System.Threading.RateLimiting;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using DomainTracker.API;
using DomainTracker.API.IoC;
using DomainTracker.API.Middleware;
using DomainTracker.Business.Abstract;
using DomainTracker.Business.Concrete;
using DomainTracker.Business.Mapping;
using DomainTracker.Core.Constants;
using DomainTracker.Core.Results;
using DomainTracker.Core.Settings;
using DomainTracker.DataAccess.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;

var nlogLogger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Host.UseServiceProviderFactory(
        new AutofacServiceProviderFactory());

    builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule(new DependencyModule());
    });

    builder.Services.AddDbContext<DomainTrackerDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddAutoMapper(_ => { }, typeof(DomainProfile).Assembly);

    builder.Services.AddControllers();

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Create;
    });

    builder.Services.AddHttpClient<IRdapClient, RdapClient>(client =>
    {
        client.BaseAddress = new Uri("https://rdap.nicproxy.com/");
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy(RateLimiterPolicies.DomainCheck, httpContext =>
        {
            var partitionKey = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                });
        });

        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = HttpStatusCodes.TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";
            var response = new ErrorResult(HttpStatusCodes.TooManyRequests, Messages.TooManyRequests);
            await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        };
    });

    if (string.IsNullOrWhiteSpace(builder.Configuration[$"{JwtSettings.SectionName}:Key"]))
    {
        throw new InvalidOperationException(Messages.JwtKeyMissing);
    }

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

    builder.Services.AddAuthorization();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter the token issued by /api/auth/login (no need to type 'Bearer ' - Swagger adds it).",
        };
        options.AddSecurityDefinition("Bearer", securityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = [],
        });
    });

    var app = builder.Build();

    await MigrateDatabaseAsync(app);

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseRouting();
    app.UseMiddleware<JwtAuthenticationMiddleware>();
    app.UseRateLimiter();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (HostAbortedException)
{
    throw;
}
catch (Exception ex)
{
    nlogLogger.Error(ex, "DomainTracker.API stopped because of an exception during startup");
    throw;
}
finally
{
    LogManager.Shutdown();
}

static async Task MigrateDatabaseAsync(WebApplication app)
{
    // Applies pending EF Core migrations on startup so docker compose up (and a fresh local
    // clone) needs no separate `dotnet ef database update step.
    const int maxAttempts = 10;
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DomainTrackerDbContext>();
            await db.Database.MigrateAsync();
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex, "Database migration attempt {Attempt}/{MaxAttempts} failed, retrying in 3s", attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

public partial class Program
{
}
