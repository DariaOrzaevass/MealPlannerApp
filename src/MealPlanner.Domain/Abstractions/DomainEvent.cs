using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Abstractions;

//Abstract basic record for domain events
public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public DateTime OccuredOnUtc { get; init; } = DateTime.UtcNow;
}
