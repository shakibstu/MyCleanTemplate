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
            await repository.CreateTest("test");
        }
    }
}