using MyCleanTemplate.Application.Contract.Abstractions.Messaging;
using MyCleanTemplate.Application.Contract.Test.Query;
using MyCleanTemplate.Domain.Test;

namespace MyCleanTemplate.Application.Test.QueryHandler
{
    public class TestQueryHandler(ITestRepository repository) : IQueryHandler<TestQuery, object>
    {
        public async Task<object> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            var x = await repository.GetAll();
            return x;
        }
    }
}
