using System.Security.Claims;
using DomainTracker.Core.Results;
using Microsoft.AspNetCore.Mvc;
using IResult = DomainTracker.Core.Results.IResult;

namespace DomainTracker.API.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [NonAction]
        public IActionResult CreateActionResult<T>(IDataResult<T> response)
        {
            return new ObjectResult(response)
            {
                StatusCode = response.StatusCode,
            };
        }

        [NonAction]
        public IActionResult CreateActionResult(IResult response)
        {
            return new ObjectResult(response)
            {
                StatusCode = response.StatusCode,
            };
        }
    }
}
