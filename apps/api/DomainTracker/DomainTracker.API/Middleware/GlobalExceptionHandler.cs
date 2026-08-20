using DomainTracker.Core.Constants;
using DomainTracker.Core.Exceptions;
using DomainTracker.Core.Results;
using Microsoft.AspNetCore.Diagnostics;

namespace DomainTracker.API.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            int statusCode;
            string message;

            if (exception is AppException appException)
            {
                statusCode = appException.StatusCode;
                message = appException.Message;

                _logger.LogWarning(
                    exception,
                    "Handled application exception on {Method} {Path}: {Message}",
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    exception.Message);
            }
            else
            {
                statusCode = HttpStatusCodes.InternalServerError;
                message = Messages.UnexpectedError;

                _logger.LogError(
                    exception,
                    "Unhandled exception on {Method} {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            var response = new ErrorResult(statusCode, message);
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
