using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Abstractions;

//Interface for domain events
public interface IDomainEvent
{
    //Id of the domain event
    Guid Id { get; }

    // The date and time of the occurrance of the domain event in UTC
    DateTime OccuredOnUtc { get; }
}
