using DomainTracker.DataAccess.Abstract;
using DomainTracker.DataAccess.Context;
using DomainTracker.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DomainTracker.DataAccess.Concrete
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(DomainTrackerDbContext context) : base(context)
        {
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            return DbSet.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
