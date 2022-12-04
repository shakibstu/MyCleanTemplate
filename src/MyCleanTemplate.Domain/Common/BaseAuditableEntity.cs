namespace MyCleanTemplate.Domain.Common
{
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public DateTime Created { get; private init; }

        public DateTime LastModified { get; set; }

        protected BaseAuditableEntity(Guid id) : base(id)
        {
            Created = DateTime.UtcNow;
        }
    }
}
