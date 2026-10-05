using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Dishes;

// Value object id for dish entity
public sealed record DishId(Guid Value)
{
    // Creates a new DishId with generating a new Guid
    public static DishId New() => new(Guid.CreateVersion7());

    // To create an DishId from existing Guid, when retrieving it from the database
    public static DishId From(Guid value) => new(value);

    // Overrides ToString to put DishId later in a readable format for database
    public override string ToString() => Value.ToString();
}
