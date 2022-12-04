using MyCleanTemplate.Domain.Test;

namespace MyCleanTemplate.Domain.Events
{
    public class TestAddedEvent : BaseEvent
    {
        public TestAddedEvent(TestEntity addedTestEntity)
        {
            AddedTestEntity = addedTestEntity;
        }

        public TestEntity AddedTestEntity { get; }
    }
}
