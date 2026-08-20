using DomainTracker.Entities.Models;

namespace DomainTracker.DataAccess.Abstract
{
    public interface IFavoriteDomainRepository : IRepository<FavoriteDomain>
    {
        Task<List<FavoriteDomain>> GetByUserIdWithDomainAsync(int userId);

        Task<FavoriteDomain?> GetByIdWithDomainAsync(int id);

        Task<bool> ExistsAsync(int userId, int domainId);
    }
}
