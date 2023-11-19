using System.Collections.Immutable;
using System.Net;

namespace MyCleanTemplate.Domain.Exceptions.Base
{
    public abstract class GlobalBaseException(string message, IDictionary<string, string[]> errors,
            HttpStatusCode statusCode)
        : Exception(message)
    {
        public HttpStatusCode StatusCode { get; } = statusCode;

        public IImmutableDictionary<string, string[]> Errors { get; } = errors.ToImmutableDictionary();
    }
}
