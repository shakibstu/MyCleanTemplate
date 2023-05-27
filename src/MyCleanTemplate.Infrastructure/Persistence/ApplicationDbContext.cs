using MediatR;
using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Infrastructure.Common;
using System.Reflection;

namespace MyCleanTemplate.Infrastructure.Persistence
{
    internal partial class ApplicationDbContext : DbContext
    {
        private readonly IMediator _mediator;
        public ApplicationDbContext(IMediator mediator, DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            _mediator = mediator;
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var changed = await base.SaveChangesAsync(cancellationToken);
            await _mediator.DispatchDomainEvents(this);
            return changed;
        }
    }
}
