using DomainTracker.Entities.Models;

namespace DomainTracker.DataAccess.Abstract
{
    public interface IDomainRepository : IRepository<Domain>
    {
        Task<Domain?> GetByNameAsync(string name);
    }
}
