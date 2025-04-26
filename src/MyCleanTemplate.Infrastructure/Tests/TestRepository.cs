using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Domain.Enums;
using MyCleanTemplate.Domain.Test;
using MyCleanTemplate.Infrastructure.Common;
using MyCleanTemplate.Infrastructure.Persistence;
namespace MyCleanTemplate.Infrastructure.Tests;

internal class TestRepository(ApplicationDbContext context) : BaseRepository<TestEntity>(context), ITestRepository
{
    private readonly DbSet<TestEntity> _entities = context.Tests;
    private readonly ApplicationDbContext _context = context;

    public Task<List<TestEntity>> GetAllAfterDate(DateTime date)
    {
        return _entities.Where(a => a.Created > date).ToListAsync();
    }

    public async Task<Guid> CreateTest(string name)
    {
        if (!_context.HasActiveTransaction)
        {
            await _context.BeginTransactionAsync();
        }

        try
        {
            var vo = TestValueObject.Create("st1", "city", "state", "country", "123");
            var testEntity = TestEntity.Create(Guid.NewGuid(), name, vo, vo, new List<TestValueObject> { vo }, TestEnum.Test0);
            await Add(testEntity);
            await _context.CommitTransactionAsync(_context.GetCurrentTransaction());
            return testEntity.Id;
        }
        catch
        {
            if (_context.HasActiveTransaction)
            {
                _context.RollbackTransaction();
            }
            throw;
        }
    }
}