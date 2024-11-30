using MyCleanTemplate.Domain.Abstractions;
using MyCleanTemplate.Domain.Enums;
using MyCleanTemplate.Domain.Test;
using ServiceGenerator;

namespace MyCleanTemplate.Infrastructure.Tests;

[TransientDependency<IDataSeeder>]
internal class TestSeeder(ITestRepository repository) : IDataSeeder
{
    public int Order { get; init; } = 1;

    public async Task SeedAsync()
    {
        if (!await repository.Any())
        {
            var vo = TestValueObject.Create("st1", "city", "state", "country", "123");
            var testEntity = TestEntity.Create(Guid.NewGuid(), "test", vo, vo, new List<TestValueObject> { vo }, TestEnum.Test0);
            await repository.Add(testEntity, true);
        }
    }
}