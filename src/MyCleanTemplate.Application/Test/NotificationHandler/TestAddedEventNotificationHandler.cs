using MediatRR.Contract.Messaging;
using Microsoft.Extensions.Logging;
using MyCleanTemplate.Domain.Events;

namespace MyCleanTemplate.Application.Test.NotificationHandler;

internal class TestAddedEventNotificationHandler(ILogger<TestAddedEventNotificationHandler> logger) : INotificationHandler<TestAddedEvent>
{
    public Task Handle(TestAddedEvent notification, CancellationToken cancellationToken)
    {

        logger.LogError("SALAM!!");
        return Task.CompletedTask;
    }
}
internal class TestAddedEventNotificationHandler1(ILogger<TestAddedEventNotificationHandler> logger) : INotificationHandler<TestAddedEvent>
{
    public Task Handle(TestAddedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogError("Aleyk!!");
        return Task.CompletedTask;
    }
}