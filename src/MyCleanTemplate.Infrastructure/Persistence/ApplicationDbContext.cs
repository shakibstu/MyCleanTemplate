using MediatR;
using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Infrastructure.Common;
using System.Reflection;

namespace MyCleanTemplate.Infrastructure.Persistence
{
    internal partial class ApplicationDbContext(IMediator mediator, DbContextOptions<ApplicationDbContext> options)
        : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var changed = await base.SaveChangesAsync(cancellationToken);
            await mediator.DispatchDomainEvents(this);
            return changed;
        }
    }
}
