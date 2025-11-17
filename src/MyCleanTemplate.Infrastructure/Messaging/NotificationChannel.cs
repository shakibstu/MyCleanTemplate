using MyCleanTemplate.Domain.Abstractions.Messaging;
using ServiceGenerator;
using System.Threading.Channels;

namespace MyCleanTemplate.Infrastructure.Messaging;

[SingletonDependency<NotificationChannel>]
internal class NotificationChannel : IDisposable
{
    private readonly Channel<NotificationPublishContext> _channel = Channel.CreateUnbounded<NotificationPublishContext>();

    public bool AddToChannel<T>(T notification, CancellationToken cancellationToken) where T : INotification
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            return _channel.Writer.TryWrite(new NotificationPublishContext(notification, notification.GetType()));

        }
        _channel.Writer.Complete();
        return false;
    }
    public IAsyncEnumerable<NotificationPublishContext> ReadFromChannel(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
    public int Count => _channel.Reader.Count;

    public void Dispose()
    {
        _channel.Writer.Complete();
    }
}