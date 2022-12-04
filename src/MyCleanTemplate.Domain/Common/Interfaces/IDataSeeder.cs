namespace MyCleanTemplate.Domain.Common.Interfaces
{
    public interface IDataSeeder : ITransientDependency
    {
        int Order { get; init;}
        Task SeedAsync();
    }
}
