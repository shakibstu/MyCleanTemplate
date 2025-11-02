namespace MyCleanTemplate.Domain.Abstractions.Messaging;

public enum MediatorActionType
{
    Request,
    RequestWithResponse,
    Command,
    CommandWithResponse,
    Notification
}