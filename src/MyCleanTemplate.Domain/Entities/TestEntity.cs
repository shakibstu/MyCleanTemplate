using MyCleanTemplate.Domain.Enums;
using MyCleanTemplate.Domain.ValueObjects;

namespace MyCleanTemplate.Domain.Entities
{
    public class TestEntity : BaseAuditableEntity
    {
        public TestEnum TestEnum { get; set; }
        public string Name { get; set; }
        public TestValueObject TestValueObject { get; set; }
        public TestValueObject TestValueObjectJson { get; set; }
        public IList<TestValueObject> TestValueObjectList { get; set; }
    }
}
