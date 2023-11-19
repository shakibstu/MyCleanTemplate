using System.Net;

namespace MyCleanTemplate.Domain.Exceptions.Base
{
    public class UnAuthorizedException(string message, IDictionary<string, string[]> data) : GlobalBaseException(message, data, HttpStatusCode.Unauthorized);
}
