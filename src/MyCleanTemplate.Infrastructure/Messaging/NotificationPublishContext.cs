using System.Diagnostics;
using MyCleanTemplate.Domain.Abstractions.Messaging;

namespace MyCleanTemplate.Infrastructure.Messaging;

internal record NotificationPublishContext(INotification Message, Type Type);