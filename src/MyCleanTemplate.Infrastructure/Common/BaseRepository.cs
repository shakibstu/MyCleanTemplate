using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Domain.Abstractions;
using MyCleanTemplate.Domain.Common;
using MyCleanTemplate.Infrastructure.Persistence;
using MyCleanTemplate.Infrastructure.Persistence.Interceptors;
using System.Linq.Expressions;
using ServiceGenerator;

namespace MyCleanTemplate.Infrastructure.Common;

[InheritableTransientInterceptInterfaces<LogRepositoryInterceptor>]
internal abstract class BaseRepository<T>(ApplicationDbContext context) : IBaseRepository<T>
    where T : BaseEntity
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public Task<int> Count()
    {
        return _dbSet.CountAsync();
    }

    public virtual async Task<bool> Any(Expression<Func<T, bool>> predicate = null)
    {
        return predicate is null ? await _dbSet.AnyAsync() : await _dbSet.AnyAsync(predicate);
    }

    public virtual async Task AddRange(IEnumerable<T> entity, bool autoSave = false)
    {
        var addRangeAsync = entity.ToList();
        _dbSet.AddRange(addRangeAsync);
        if (autoSave)
        {
            await context.SaveChangesAsync();
        }
    }

    public virtual async Task Add(T entity, bool autoSave = false)
    {
        _dbSet.Add(entity); 
        if (autoSave)
        {
            await context.SaveChangesAsync();
        }
    }

    public virtual async Task Update(T entity, bool autoSave = false)
    {
        _dbSet.Attach(entity);
        context.Entry(entity).State = EntityState.Modified; 
        if (autoSave)
        {
            await context.SaveChangesAsync();
        }
    }

    public virtual async Task<T> Get(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> GetFilteredAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<List<T>> GetAll()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task Delete(Guid id, bool autoSave = false)
    {
        var entity = await Get(id);
        if (entity is not null)
        {
            if (context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
            }
            _dbSet.Remove(entity);
        }
        if (autoSave)
        {
            await context.SaveChangesAsync();
        }
    }
}