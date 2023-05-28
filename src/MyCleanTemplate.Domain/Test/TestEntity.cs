using MyCleanTemplate.Domain.Enums;

namespace MyCleanTemplate.Domain.Test
{
    public class TestEntity : BaseAuditableEntity
    {
        private TestEntity(Guid id) : base(id) { }
        private TestEntity(Guid id, string name, TestValueObject testValueObject, TestValueObject testValueObjectJson, List<TestValueObject> testValueObjectList, TestEnum testEnum) : base(id)
        {
            Name = name;
            TestValueObject = testValueObject;
            TestValueObjectJson = testValueObjectJson;
            TestValueObjectList = testValueObjectList;
            TestEnum = testEnum;
        }

        public TestEnum TestEnum { get; private set; }
        public string Name { get; private set; }
        public TestValueObject TestValueObject { get; private set; }
        public TestValueObject TestValueObjectJson { get; private set; }
        public List<TestValueObject> TestValueObjectList { get; private set; }

        public static TestEntity Create(Guid id, string name, TestValueObject testValueObject, TestValueObject testValueObjectJson, List<TestValueObject> testValueObjectList, TestEnum testEnum)
        {
            return new TestEntity(id, name, testValueObject, testValueObjectJson, testValueObjectList, testEnum);
        }
    }
}
