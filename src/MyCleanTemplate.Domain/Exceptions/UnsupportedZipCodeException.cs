
using MyCleanTemplate.Domain.Exceptions.Base;

namespace MyCleanTemplate.Domain.Exceptions
{
    public class UnsupportedZipCodeException(string zipCode) : BadRequestException($"ZipCode \"{zipCode}\" is unsupported.", new Dictionary<string, string[]>());
}
