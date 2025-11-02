using ServiceGenerator;

namespace MyCleanTemplate.Domain.Abstractions.Messaging;

/// <summary>
/// Defines a handler for a notification
/// </summary>
/// <typeparam name="TNotification">The type of notification being handled</typeparam>
[InheritableTransientInterfaces]
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles a notification
    /// </summary>
    /// <param name="notification">The notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}

