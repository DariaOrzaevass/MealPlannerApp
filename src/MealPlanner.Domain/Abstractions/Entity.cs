using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Abstractions;

// Basic abstract class for all entities
public abstract class Entity<TEntityId> : IEntity
    where TEntityId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    protected Entity(TEntityId id) => Id = id;

    // Default constructor for EF core to get things from database
    protected Entity()
    {

    }

    // Entity id
    public TEntityId Id { get; init; } = default!;

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    
    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
