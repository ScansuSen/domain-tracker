using DomainTracker.Core.Constants;
using DomainTracker.Core.Results;
using Microsoft.AspNetCore.Mvc;

namespace DomainTracker.API.Middleware
{
    public static class InvalidModelStateResponseFactory
    {
        public static IActionResult Create(ActionContext context)
        {
            var messages = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry => entry.Value!.Errors.Select(e => $"{entry.Key}: {e.ErrorMessage}"))
                .ToList();

            var response = new ErrorResult(HttpStatusCodes.BadRequest, messages);
            return new ObjectResult(response) { StatusCode = HttpStatusCodes.BadRequest };
        }
    }
}
