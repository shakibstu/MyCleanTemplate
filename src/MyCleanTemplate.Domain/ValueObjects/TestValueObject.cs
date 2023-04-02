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
        private TestValueObject() { }
        private TestValueObject(string street, string city, string state, string country, string zipCode)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            ZipCode = zipCode;
        }
        public static TestValueObject Create(string street, string city, string state, string country, string zipCode)
        {
            if (zipCode.Length < 3)
                throw new UnsupportedZipCodeException(zipCode);
            return new TestValueObject(street, city, state, country, zipCode);
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
