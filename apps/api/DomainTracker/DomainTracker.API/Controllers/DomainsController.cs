using DomainTracker.Business.Abstract;
using DomainTracker.DTOs.Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DomainTracker.API.Controllers
{
  
    [Authorize]
    [Route("api/[controller]")]
    public class DomainsController : ApiControllerBase
    {
        private readonly IDomainService _domainService;

        public DomainsController(IDomainService domainService)
        {
            _domainService = domainService;
        }

        [HttpGet("check")]
        [EnableRateLimiting(RateLimiterPolicies.DomainCheck)]
        public async Task<IActionResult> Check([FromQuery] DomainCheckRequestDto request)
        {
            var result = await _domainService.CheckAsync(request.Name);
            return CreateActionResult(result);
        }
    }
}
