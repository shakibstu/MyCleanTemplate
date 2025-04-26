using MyCleanTemplate.Domain.Abstractions;

namespace MyCleanTemplate.Domain.Test;

public interface ITestRepository : IBaseRepository<TestEntity>
{
    Task<List<TestEntity>> GetAllAfterDate(DateTime date);
    Task<Guid> CreateTest(string name);
}