using System.Net;

namespace MyCleanTemplate.Domain.Exceptions.Base
{
    public class UnAuthorizedException : GlobalBaseException
    {
        public UnAuthorizedException(string message, IDictionary<string, string[]> data) : base(message, data, HttpStatusCode.Unauthorized) { }
    }
}
