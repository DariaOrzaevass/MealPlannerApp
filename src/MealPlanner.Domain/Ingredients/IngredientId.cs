using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Ingredients;

// Value object id for ingredient entity
public sealed record IngredientId(Guid Value)
{
    // Creates a new IngredientId with generating a new Guid
    public static IngredientId New() => new(Guid.CreateVersion7());

    // To create an IngredientId from existing Guid, when retrieving it from the database
    public static IngredientId From(Guid value) => new(value);

    // Overrides ToString to put IngredientId later in a readable format for database
    public override string ToString() => Value.ToString();
}
