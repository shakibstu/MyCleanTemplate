using MyCleanTemplate.Domain.Common.Interfaces;

namespace MyCleanTemplate.Domain.Test
{
    public interface ITestRepository : IBaseRepository<TestEntity>
    {
        Task<List<TestEntity>> GetAllAfterDate(DateTime date);
    }
}
