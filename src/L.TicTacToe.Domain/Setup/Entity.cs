namespace L.TicTacToe.Domain.Setup;

public abstract class Entity<TIdentifier> where TIdentifier : IEquatable<TIdentifier>
{
    int? _requestedHashCode;

    TIdentifier? _id;
    public virtual TIdentifier? Id
    {
        get => _id;
        protected set => _id = value;
    }

    private List<IDomainEvent>? _domainEvents;
    public IReadOnlyCollection<IDomainEvent> DomainEvents => 
        _domainEvents?.AsReadOnly() ?? Array.Empty<IDomainEvent>().AsReadOnly();

    public abstract void ApplyEvent(IDomainEvent @event);

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= [];
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents?.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }

    public bool IsTransient()
    {
        return Id == null || Id.Equals(default);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not Entity<TIdentifier>)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (GetType() != obj.GetType())
            return false;

        Entity<TIdentifier> item = (Entity<TIdentifier>)obj;

        if (item.IsTransient() || IsTransient())
            return false;
        else
            return item.Id!.Equals(Id);
    }

    public override int GetHashCode()
    {
        if (!IsTransient())
        {
            if (!_requestedHashCode.HasValue)
                _requestedHashCode = Id!.GetHashCode() ^ 31;
            return _requestedHashCode.Value;
        }
        else
            return base.GetHashCode();
    }

    public static bool operator ==(Entity<TIdentifier>? left, Entity<TIdentifier>? right)
    {
        if (Equals(left, null))
            return Equals(right, null);
        else
            return left.Equals(right);
    }

    public static bool operator !=(Entity<TIdentifier>? left, Entity<TIdentifier>? right)
    {
        return !(left == right);
    }
}