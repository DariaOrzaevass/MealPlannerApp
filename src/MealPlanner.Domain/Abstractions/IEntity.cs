using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Abstractions;

//All entities will implement this interface
internal interface IEntity
{
    //Returns the domain events of the entity
    IReadOnlyCollection<IDomainEvent> GetDomainEvents();

    //Clears the domain events of the entity
    void ClearDomainEvents();

}
