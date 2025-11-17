
using MyCleanTemplate.Domain.Abstractions.Messaging;

namespace MyCleanTemplate.Domain.Common;

public abstract class BaseEvent : INotification
{
    public Dictionary<string, object> Properties { get; set; }
}