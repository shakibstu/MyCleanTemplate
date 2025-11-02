using Microsoft.Identity.Client;
using MyCleanTemplate.Domain.Abstractions.Messaging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using ServiceGenerator;
using System.Diagnostics;
using System.Threading.Channels;

namespace MyCleanTemplate.Infrastructure.Messaging;

[SingletonDependency<NotificationChannel>]
internal class NotificationChannel : IDisposable
{
    private readonly Channel<NotificationPublishContext> _channel = Channel.CreateUnbounded<NotificationPublishContext>();

    public bool AddToChannel<T>(T notification,Activity activity, CancellationToken cancellationToken) where T : INotification
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            var properties = new Dictionary<string, string>();
            var propagationContext = new PropagationContext(
                activity?.Context ?? default,
                Baggage.Current);
            Propagators.DefaultTextMapPropagator.Inject(propagationContext, properties, (dictionary, key, value) =>
            {
                dictionary[key] = value;
            });
            return _channel.Writer.TryWrite(new NotificationPublishContext(notification, notification.GetType(), properties));

        }
        _channel.Writer.Complete();
        return false;
    }
    public IAsyncEnumerable<NotificationPublishContext> ReadFromChannel(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
    public void Dispose()
    {
        _channel.Writer.Complete();
    }
}