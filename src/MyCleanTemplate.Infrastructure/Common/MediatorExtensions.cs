
using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using MyCleanTemplate.Domain.Common;

namespace MyCleanTemplate.Infrastructure.Common;

internal static class MediatorExtensions
{
    public static async Task DispatchDomainEvents(this IMediator mediator, DbContext context)
    {
        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var publishTasks = entities
            .SelectMany(e => e.DomainEvents)
            .Select(evt => mediator.Publish(evt));
        await Task.WhenAll(publishTasks);
        entities.ForEach(e => e.ClearDomainEvents());
    }

}