using MyCleanTemplate.Common.Dependencies;
using MyCleanTemplate.Domain.Abstractions;
using MyCleanTemplate.Domain.Enums;
using MyCleanTemplate.Domain.Test;

namespace MyCleanTemplate.Infrastructure.Tests
{
    internal class TestSeeder : IDataSeeder, ITransientDependency
    {
        private readonly ITestRepository _repository;
        public TestSeeder(ITestRepository repository)
        {
            _repository = repository;
            Order = 1;
        }
        public int Order { get; init; }
        public async Task SeedAsync()
        {
            if (!await _repository.Any())
            {
                var vo = TestValueObject.Create("st1", "city", "state", "country", "123");
                var testEntity = TestEntity.Create(Guid.NewGuid(), "test", vo, vo, new List<TestValueObject> { vo }, TestEnum.Test0);
                await _repository.Add(testEntity, true);
            }
        }
    }
}
