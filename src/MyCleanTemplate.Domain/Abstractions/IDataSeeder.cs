namespace MyCleanTemplate.Domain.Abstractions;

public interface IDataSeeder
{
    int Order { get; init; }
    Task SeedAsync();
}