using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Domain.Test;
using MyCleanTemplate.Infrastructure.Common;
using MyCleanTemplate.Infrastructure.Persistence;
using MyCleanTemplate.Infrastructure.Persistence.Interceptors;
using ServiceGenerator;

namespace MyCleanTemplate.Infrastructure.Tests;

internal class TestRepository(ApplicationDbContext context) : BaseRepository<TestEntity>(context), ITestRepository
{
    private readonly DbSet<TestEntity> _entities = context.Tests;

    public Task<List<TestEntity>> GetAllAfterDate(DateTime date)
    {
        return _entities.Where(a => a.Created > date).ToListAsync();
    }
}