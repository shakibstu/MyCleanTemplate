using System.Net;

namespace MyCleanTemplate.Domain.Exceptions.Base
{
    public class ForbiddenException : GlobalBaseException
    {
        public ForbiddenException(string message, IDictionary<string, string[]> data) : base(message, data, HttpStatusCode.Forbidden) { }
    }
}
