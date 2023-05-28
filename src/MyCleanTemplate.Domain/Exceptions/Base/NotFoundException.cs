using System.Net;

namespace MyCleanTemplate.Domain.Exceptions.Base
{
    public class NotFoundException : GlobalBaseException
    {
        public NotFoundException(string message, IDictionary<string, string[]> data) : base(message, data, HttpStatusCode.NotFound) { }
    }
}
