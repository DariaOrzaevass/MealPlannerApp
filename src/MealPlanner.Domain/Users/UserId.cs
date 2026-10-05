using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Users;

// Value object id for user entity
public sealed record UserId(Guid Value)
{
    // Creates a new UserId with generating a new Guid
    public static UserId New() => new(Guid.CreateVersion7());

    // To create an UserId from existing Guid, when retrieving it from the database
    public static UserId From(Guid value) => new(value);

    // Overrides ToString to put UserId later in a readable format for database
    public override string ToString() => Value.ToString();
}
