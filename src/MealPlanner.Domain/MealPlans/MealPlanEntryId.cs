using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.MealPlans;

// Value object id for MealPlanEntry
public sealed record MealPlanEntryId(Guid Value)
{
    // Creates a new MealPlanEntryId with generating a new Guid
    public static MealPlanEntryId New() => new(Guid.CreateVersion7());

    // To create an MealPlanEntryId from existing Guid, when retrieving it from the database
    public static MealPlanEntryId From(Guid value) => new(value);

    // Overrides ToString to put MealPlanEntryId later in a readable format for database
    public override string ToString() => Value.ToString();
}
