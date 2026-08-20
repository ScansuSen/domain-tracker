using DomainTracker.DataAccess.Abstract;
using DomainTracker.DataAccess.Context;
using DomainTracker.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DomainTracker.DataAccess.Concrete
{
    public class DomainRepository : RepositoryBase<Domain>, IDomainRepository
    {
        public DomainRepository(DomainTrackerDbContext context) : base(context)
        {
        }

        public Task<Domain?> GetByNameAsync(string name)
        {
            return DbSet.FirstOrDefaultAsync(d => d.Name == name);
        }
    }
}
