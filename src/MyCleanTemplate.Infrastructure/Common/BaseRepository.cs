using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Domain.Common;
using MyCleanTemplate.Domain.Common.Interfaces;
using MyCleanTemplate.Infrastructure.Persistence;

namespace MyCleanTemplate.Infrastructure.Common
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        protected BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public Task<int> Count()
        {
            return _dbSet.CountAsync();
        }

        public Task<bool> Any(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate is null)
                return _dbSet.AnyAsync();
            return _dbSet.AnyAsync(predicate);
        }

        public Task AddRange(IEnumerable<T> entity, bool autoSave = false)
        {
            var addRangeAsync = entity.ToList();
            _dbSet.AddRange(addRangeAsync);
            if (autoSave)
            {
                return _context.SaveChangesAsync();
            }
            return Task.CompletedTask;
        }

        public Task Add(T entity, bool autoSave = false)
        {
            _dbSet.Add(entity);
            if (autoSave)
            {
                return _context.SaveChangesAsync();
            }
            return Task.CompletedTask;
        }

        public Task Update(T entity, bool autoSave = false)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            if (autoSave)
            {
                return _context.SaveChangesAsync();
            }
            return Task.CompletedTask;
        }

        public async Task<T> Get(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<List<T>> GetFilteredAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public Task<List<T>> GetAll()
        {
            return _dbSet.ToListAsync();
        }

        public async Task Delete(Guid id, bool autoSave = false)
        {
            var entity = await Get(id);
            if (entity is not null)
            {
                if (_context.Entry(entity).State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }
                _dbSet.Remove(entity);
            }
            if (autoSave)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
