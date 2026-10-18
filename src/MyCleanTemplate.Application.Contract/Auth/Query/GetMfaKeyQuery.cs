using MyCleanTemplate.Domain.Abstractions.Messaging;

namespace MyCleanTemplate.Application.Contract.Auth.Query;

public sealed record GetMfaKeyQuery(string UserId) : IQuery<string>;
