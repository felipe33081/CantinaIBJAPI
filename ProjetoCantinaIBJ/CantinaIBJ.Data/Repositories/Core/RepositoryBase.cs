using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts.Core;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories.Core
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly PostgreSqlContext Context;

        protected RepositoryBase(PostgreSqlContext context)
        {
            Context = context;
        }

        public async virtual Task<T> AddAsync(T obj)
        {
            await Context.Set<T>().AddAsync(obj);
            await Context.SaveChangesAsync();
            return obj;
        }

        public async virtual Task AddNoCommitAsync(T obj)
        {
            await Context.Set<T>().AddAsync(obj);
            return;
        }

        public void UpdateNoCommit(T obj)
        {
            Context.Set<T>().Update(obj);
        }

        public async Task<T> GetAsync(T obj)
        {
            return await Context.Set<T>().FindAsync(obj);
        }

        public async Task<T> GetAsync(Guid id)
        {
            return await Context.Set<T>().FindAsync(id);
        }

        public async Task<List<T>> ListAsync()
        {
            return await Context.Set<T>().ToListAsync();
        }

        public async Task DeleteAsync(T obj)
        {
            Context.Set<T>().Remove(obj);
            await Context.SaveChangesAsync();
            return;
        }

        public async Task DeleteAsync(int id)
        {
            Context.Set<T>().Remove(Context.Set<T>().Find(id));
            await Context.SaveChangesAsync();
            return;
        }

        public async Task<T> UpdateAsync(T obj)
        {
            Context.Set<T>().Update(obj);
            await Context.SaveChangesAsync();
            return obj;
        }

        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync();
        }
    }
}
