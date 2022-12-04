using System.Linq.Expressions;

namespace MyCleanTemplate.Domain.Common.Interfaces
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        Task<int> Count();
        Task<bool> Any(Expression<Func<T, bool>> predicate = null);
        Task AddRange(IEnumerable<T> entity, bool autoSave = false);
        Task Add(T entity, bool autoSave = false);
        Task Update(T entity, bool autoSave = false);
        Task<T> Get(Guid id);
        Task<List<T>> GetAll();
        Task Delete(Guid id, bool autoSave = false);
    }
}
