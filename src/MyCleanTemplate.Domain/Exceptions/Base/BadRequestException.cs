using System.Net;

namespace MyCleanTemplate.Domain.Exceptions.Base
{
    public class BadRequestException(string message, IDictionary<string, string[]> data) : GlobalBaseException(message, data, HttpStatusCode.BadRequest);
}
