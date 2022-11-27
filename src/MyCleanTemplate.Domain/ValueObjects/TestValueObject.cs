using MyCleanTemplate.Domain.Exceptions;

namespace MyCleanTemplate.Domain.ValueObjects
{
    public class TestValueObject : ValueObject
    {
        public string Street { get; }
        public string City { get; }
        public string State { get; }
        public string Country { get; }
        public string ZipCode { get; }
        public TestValueObject(string street, string city, string state, string country, string zipCode)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            if (zipCode.Length < 3)
                throw new UnsupportedZipCodeException(zipCode);
            ZipCode = zipCode;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return State;
            yield return Country;
            yield return ZipCode;
        }
    }
}
