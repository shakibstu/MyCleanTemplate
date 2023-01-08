using System.Collections;
using System.Collections.Immutable;
using System.Net;

namespace MyCleanTemplate.Domain.Exceptions.Base
{
    public abstract class GlobalBaseException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public IImmutableDictionary<string, string[]> Errors { get; }

        protected GlobalBaseException(string message, IDictionary<string, string[]> errors, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
            Errors = errors.ToImmutableDictionary();
        }

    }
}
