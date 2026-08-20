using DomainTracker.Business.Abstract;
using DomainTracker.DTOs.Domains;
using Microsoft.AspNetCore.Mvc;

namespace DomainTracker.API.Controllers
{
    [Route("api/[controller]")]
    public class DomainsController : ApiControllerBase
    {
        private readonly IDomainService _domainService;

        public DomainsController(IDomainService domainService)
        {
            _domainService = domainService;
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check([FromQuery] DomainCheckRequestDto request)
        {
            var result = await _domainService.CheckAsync(request.Name);
            return CreateActionResult(result);
        }
    }
}
