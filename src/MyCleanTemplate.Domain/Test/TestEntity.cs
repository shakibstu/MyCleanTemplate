using MyCleanTemplate.Domain.Enums;
using MyCleanTemplate.Domain.ValueObjects;

namespace MyCleanTemplate.Domain.Test
{
    public class TestEntity : BaseAuditableEntity
    {
        private readonly List<TestValueObject> _values;
        public TestEnum TestEnum { get; private set; }
        public string Name { get; private set; }
        public TestValueObject TestValueObject { get; private set; }
        public TestValueObject TestValueObjectJson { get; private set; }
        public IReadOnlyCollection<TestValueObject> TestValueObjectList => _values;

        public TestEntity(Guid id, string name, TestValueObject testValueObject, TestValueObject testValueObjectJson, List<TestValueObject> testValueObjectList, TestEnum testEnum) : base(id)
        {
            Name = name;
            TestValueObject = testValueObject;
            TestValueObjectJson = testValueObjectJson;
            _values = testValueObjectList;
            TestEnum = testEnum;
        }
    }
}
