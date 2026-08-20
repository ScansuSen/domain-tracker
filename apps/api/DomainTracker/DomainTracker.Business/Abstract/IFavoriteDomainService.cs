using DomainTracker.Core.Results;
using DomainTracker.DTOs.Favorites;

namespace DomainTracker.Business.Abstract
{
    public interface IFavoriteDomainService
    {
        Task<IDataResult<List<FavoriteDomainResponseDto>>> GetAllForUserAsync(int userId);

        Task<IDataResult<FavoriteDomainResponseDto>> AddAsync(int userId, string domainName);

        Task<IResult> DeleteAsync(int userId, int favoriteId);

        Task<IDataResult<FavoriteDomainResponseDto>> RefreshAsync(int userId, int favoriteId);
    }
}
