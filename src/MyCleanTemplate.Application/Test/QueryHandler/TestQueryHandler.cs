using MyCleanTemplate.Application.Contract.Abstractions.Messaging;
using MyCleanTemplate.Application.Contract.Test.Query;
using MyCleanTemplate.Domain.Test;

namespace MyCleanTemplate.Application.Test.QueryHandler
{
    public class TestQueryHandler : IQueryHandler<TestQuery, object>
    {
        private readonly ITestRepository _repository;
        public TestQueryHandler(ITestRepository repository)
        {
            _repository = repository;
        }
        public async Task<object> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            var x = await _repository.GetAll();
            return x;
        }
    }
}
