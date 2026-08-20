using DomainTracker.Business.Abstract;
using DomainTracker.DTOs.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomainTracker.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class FavoritesController : ApiControllerBase
    {
        private readonly IFavoriteDomainService _favoriteDomainService;

        public FavoritesController(IFavoriteDomainService favoriteDomainService)
        {
            _favoriteDomainService = favoriteDomainService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _favoriteDomainService.GetAllForUserAsync(CurrentUserId);
            return CreateActionResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddFavoriteRequestDto request)
        {
            var result = await _favoriteDomainService.AddAsync(CurrentUserId, request.DomainName);
            return CreateActionResult(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _favoriteDomainService.DeleteAsync(CurrentUserId, id);
            return CreateActionResult(result);
        }

        [HttpPost("{id:int}/refresh")]
        public async Task<IActionResult> Refresh(int id)
        {
            var result = await _favoriteDomainService.RefreshAsync(CurrentUserId, id);
            return CreateActionResult(result);
        }
    }
}
