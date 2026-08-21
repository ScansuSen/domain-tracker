using DomainTracker.DataAccess.Abstract;
using DomainTracker.DataAccess.Context;
using DomainTracker.DataAccess.Enums;
using DomainTracker.DataAccess.Models;
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
                .AsNoTracking()
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

        public async Task<(AddFavoriteOutcome Outcome, FavoriteDomain? Favorite)> AddFavoriteAsync(int userId, string domainName, DomainCheckInfo checkInfo)
        {
            var domain = await Context.Domains.FirstOrDefaultAsync(d => d.Name == domainName);

            if (domain is not null && await DbSet.AnyAsync(f => f.UserId == userId && f.DomainId == domain.Id))
                return (AddFavoriteOutcome.AlreadyFavorited, null);

            if (domain is null)
            {
                domain = new Domain
                {
                    Name = domainName,
                    IsAvailable = checkInfo.IsAvailable,
                    LastCheckedAt = checkInfo.LastCheckedAt,
                    ExpirationDate = checkInfo.ExpirationDate,
                };
                await Context.Domains.AddAsync(domain);
            }
            else
            {
                domain.IsAvailable = checkInfo.IsAvailable;
                domain.LastCheckedAt = checkInfo.LastCheckedAt;
                domain.ExpirationDate = checkInfo.ExpirationDate;
                domain.UpdatedAt = checkInfo.LastCheckedAt;
            }

            var favorite = new FavoriteDomain { UserId = userId, Domain = domain };
            await DbSet.AddAsync(favorite);

            await Context.SaveChangesAsync();

            return (AddFavoriteOutcome.Created, favorite);
        }

        public Task RefreshDomainAsync(FavoriteDomain favorite, DomainCheckInfo checkInfo)
        {
            favorite.Domain.IsAvailable = checkInfo.IsAvailable;
            favorite.Domain.LastCheckedAt = checkInfo.LastCheckedAt;
            favorite.Domain.ExpirationDate = checkInfo.ExpirationDate;
            favorite.Domain.UpdatedAt = checkInfo.LastCheckedAt;

            return Context.SaveChangesAsync();
        }
    }
}
