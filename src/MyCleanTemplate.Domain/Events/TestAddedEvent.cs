using MyCleanTemplate.Domain.Test;

namespace MyCleanTemplate.Domain.Events
{
    public class TestAddedEvent(TestEntity addedTestEntity) : BaseEvent
    {
        public TestEntity AddedTestEntity { get; } = addedTestEntity;
    }
}
