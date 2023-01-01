using Microsoft.EntityFrameworkCore.Storage;
using MyCleanTemplate.Common.Dependencies;
using MyCleanTemplate.Domain.Abstractions;
using MyCleanTemplate.Infrastructure.Persistence;

namespace MyCleanTemplate.Infrastructure.Common
{
    internal class UnitOfWork : IUnitOfWork, IScopeDependency
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return _context.Database.BeginTransactionAsync(cancellationToken);
        }
    }
}
