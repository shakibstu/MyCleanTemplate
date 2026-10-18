using System.Collections.Concurrent;
using MediatRR.Contract.Messaging;
using MyCleanTemplate.Domain.Events;

namespace MyCleanTemplate.IntegrationTest.Automation;

/// <summary>
/// Records domain events that reached a notification handler. Handlers run on MediatRR's
/// background worker, so tests wait on <see cref="WaitForAsync"/> instead of asserting immediately.
/// </summary>
public sealed class NotificationProbe
{
    private readonly ConcurrentQueue<INotification> _received = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void Record(INotification notification)
    {
        _received.Enqueue(notification);
        _signal.Release();
    }

    public async Task<T?> WaitForAsync<T>(Func<T, bool> match, TimeSpan timeout, CancellationToken cancellationToken)
        where T : class, INotification
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        try
        {
            while (true)
            {
                var found = _received.OfType<T>().FirstOrDefault(match);
                if (found is not null) return found;
                await _signal.WaitAsync(cts.Token);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }
}

public sealed class RecordingTestAddedEventHandler(NotificationProbe probe) : INotificationHandler<TestAddedEvent>
{
    public Task Handle(TestAddedEvent notification, CancellationToken cancellationToken)
    {
        probe.Record(notification);
        return Task.CompletedTask;
    }
}
