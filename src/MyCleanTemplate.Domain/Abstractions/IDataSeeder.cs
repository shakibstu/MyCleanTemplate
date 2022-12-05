namespace MyCleanTemplate.Domain.Abstractions
{
    public interface IDataSeeder : ITransientDependency
    {
        int Order { get; init;}
        Task SeedAsync();
    }
}
