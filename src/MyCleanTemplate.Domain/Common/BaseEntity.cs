using System.ComponentModel.DataAnnotations.Schema;

namespace MyCleanTemplate.Domain.Common
{
    public abstract class BaseEntity(Guid id) : IEquatable<BaseEntity>
    {

        public Guid Id { get; private init; } = id;

        private readonly List<BaseEvent> _domainEvents = new();

        [NotMapped]
        public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(BaseEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(BaseEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (obj is not BaseEntity entity)
                return false;

            return entity.Id == Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_domainEvents, Id);
        }

        public bool Equals(BaseEntity other)
        {
            if (other is null)
                return false;

            if (other.GetType() != GetType())
                return false;

            return other.Id == Id;
        }

        public static bool operator ==(BaseEntity first, BaseEntity second) =>
            first is not null && second is not null && first.Equals(second);

        public static bool operator !=(BaseEntity first, BaseEntity second) => !(first == second);
    }
}
