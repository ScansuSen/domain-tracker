using DomainTracker.Entities.Models;

namespace DomainTracker.DataAccess.Abstract
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
    }
}
