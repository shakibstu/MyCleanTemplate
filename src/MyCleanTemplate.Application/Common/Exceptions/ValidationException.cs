using MyCleanTemplate.Domain.Exceptions.Base;

namespace MyCleanTemplate.Application.Common.Exceptions
{
    public class ValidationException : BadRequestException
    {
        public ValidationException(string message, IDictionary<string, string[]> data) : base(message, data)
        {
        }
    }
}
