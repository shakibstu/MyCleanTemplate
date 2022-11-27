
namespace MyCleanTemplate.Domain.Exceptions
{
    public class UnsupportedZipCodeException : Exception
    {
        public UnsupportedZipCodeException(string zipCode) : base($"ZipCode \"{zipCode}\" is unsupported.")
        {

        }
    }
}
