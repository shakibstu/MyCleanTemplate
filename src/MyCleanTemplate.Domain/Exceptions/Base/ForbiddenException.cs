using System.Net;

namespace MyCleanTemplate.Domain.Exceptions.Base;

public class ForbiddenException(string message, IDictionary<string, string[]> data) : GlobalBaseException(message, data, HttpStatusCode.Forbidden);