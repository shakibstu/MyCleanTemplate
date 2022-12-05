using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Domain.Test;

namespace MyCleanTemplate.Infrastructure.Persistence
{
    internal partial class ApplicationDbContext
    {
        public DbSet<TestEntity> Tests => Set<TestEntity>();
    }
}
