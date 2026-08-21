using DomainTracker.DataAccess.Enums;
using DomainTracker.DataAccess.Models;
using DomainTracker.Entities.Models;

namespace DomainTracker.DataAccess.Abstract
{
    public interface IFavoriteDomainRepository : IRepository<FavoriteDomain>
    {
        Task<List<FavoriteDomain>> GetByUserIdWithDomainAsync(int userId);

        Task<FavoriteDomain?> GetByIdWithDomainAsync(int id);

        Task<(AddFavoriteOutcome Outcome, FavoriteDomain? Favorite)> AddFavoriteAsync(int userId, string domainName, DomainCheckInfo checkInfo);

        Task RefreshDomainAsync(FavoriteDomain favorite, DomainCheckInfo checkInfo);
    }
}
