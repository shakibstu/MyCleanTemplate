using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Common.Dependencies;
using MyCleanTemplate.Domain.Test;
using MyCleanTemplate.Infrastructure.Common;
using MyCleanTemplate.Infrastructure.Persistence;

namespace MyCleanTemplate.Infrastructure.Tests
{
    internal class TestRepository : BaseRepository<TestEntity>, ITestRepository, IScopeDependency
    {
        private readonly DbSet<TestEntity> _entities;
        public TestRepository(ApplicationDbContext context) : base(context)
        {
            _entities = context.Tests;
        }

        public Task<List<TestEntity>> GetAllAfterDate(DateTime date)
        {
            return _entities.Where(a => a.Created > date).ToListAsync();
        }
    }
}
