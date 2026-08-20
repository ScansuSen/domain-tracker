using DomainTracker.DataAccess.Abstract;
using DomainTracker.DataAccess.Context;
using DomainTracker.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DomainTracker.DataAccess.Concrete
{
    public class FavoriteDomainRepository : RepositoryBase<FavoriteDomain>, IFavoriteDomainRepository
    {
        public FavoriteDomainRepository(DomainTrackerDbContext context) : base(context)
        {
        }

        public Task<List<FavoriteDomain>> GetByUserIdWithDomainAsync(int userId)
        {
            return DbSet
                .Include(f => f.Domain)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public Task<FavoriteDomain?> GetByIdWithDomainAsync(int id)
        {
            return DbSet
                .Include(f => f.Domain)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public Task<bool> ExistsAsync(int userId, int domainId)
        {
            return DbSet.AnyAsync(f => f.UserId == userId && f.DomainId == domainId);
        }
    }
}
