using MediatRR.Contract.Messaging;

namespace MyCleanTemplate.Infrastructure.Messaging;

internal record NotificationPublishContext(INotification Message, Type Type);