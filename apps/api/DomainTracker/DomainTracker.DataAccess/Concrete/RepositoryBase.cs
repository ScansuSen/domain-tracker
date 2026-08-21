using System.Linq;
using DomainTracker.DataAccess.Abstract;
using DomainTracker.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainTracker.DataAccess.Concrete
{
    public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DomainTrackerDbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        protected RepositoryBase(DomainTrackerDbContext context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            var alreadyTracked = Context.ChangeTracker.Entries<TEntity>().Any(e => ReferenceEquals(e.Entity, entity));
            if (!alreadyTracked)
                DbSet.Update(entity);
            await Context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            DbSet.Remove(entity);
            await Context.SaveChangesAsync();
        }
    }
}
