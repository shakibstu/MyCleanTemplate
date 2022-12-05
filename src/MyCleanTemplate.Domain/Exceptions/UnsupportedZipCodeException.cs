
using MyCleanTemplate.Domain.Exceptions.Base;

namespace MyCleanTemplate.Domain.Exceptions
{
    public class UnsupportedZipCodeException : BadRequestException
    {
        public UnsupportedZipCodeException(string zipCode) : base($"ZipCode \"{zipCode}\" is unsupported.")
        {

        }
    }
}
