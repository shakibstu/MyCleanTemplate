using System.Net;

namespace MyCleanTemplate.Domain.Exceptions.Base
{
    public class NotFoundException(string message, IDictionary<string, string[]> data) : GlobalBaseException(message, data, HttpStatusCode.NotFound);
}
