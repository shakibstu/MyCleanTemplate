namespace MyCleanTemplate.Domain.Common;

public abstract class BaseAuditableEntity(Guid id) : BaseEntity(id)
{
    public DateTime Created { get; private init; } = DateTime.UtcNow;

    public DateTime? LastModified { get; set; }
}