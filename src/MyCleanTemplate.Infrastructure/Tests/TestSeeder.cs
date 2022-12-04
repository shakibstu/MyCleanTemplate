using MyCleanTemplate.Domain.Common.Interfaces;
using MyCleanTemplate.Domain.Enums;
using MyCleanTemplate.Domain.Test;
using MyCleanTemplate.Domain.ValueObjects;

namespace MyCleanTemplate.Infrastructure.Tests
{
    public class TestSeeder : IDataSeeder
    {
        private readonly ITestRepository _repository;
        public TestSeeder(ITestRepository repository)
        {
            _repository = repository;
            Order = 1;
        }
        public int Order { get; init; }
        public Task SeedAsync()
        {
            var vo = TestValueObject.Create("st1", "city", "state", "country", "123");
            var testEntity = TestEntity.Create(Guid.NewGuid(), "test", vo,vo,new List<TestValueObject>{ vo }, TestEnum.Test0 );
            return _repository.Add(testEntity, true);
        }
    }
}
