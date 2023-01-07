using System.Net;

namespace MyCleanTemplate.Domain.Exceptions.Base
{
    public class BadRequestException : GlobalBaseException
    {
        public BadRequestException(string message, IDictionary<string, string[]> data) : base(message, data, HttpStatusCode.BadRequest)
        {
        }
    }
}
