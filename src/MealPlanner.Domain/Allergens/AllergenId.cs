using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Allergens;

// Value object id for allergen entity
public sealed record AllergenId(Guid Value)
{
    // Creates a new AllergenId with generating a new Guid
    public static AllergenId New() => new(Guid.CreateVersion7());

    // To create an AllergenId from existing Guid, when retrieving it from the database
    public static AllergenId From(Guid value) => new(value);

    // Overrides ToString to put AllergenId later in a readable format for database
    public override string ToString() => Value.ToString();
}
