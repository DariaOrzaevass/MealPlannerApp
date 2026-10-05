using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.MealPlans;

// Value object id for MealPlan entity
public sealed record MealPlanId(Guid Value)
{
    // Creates a new MealPlanId with generating a new Guid
    public static MealPlanId New() => new(Guid.CreateVersion7());

    // To create an MealPlanId from existing Guid, when retrieving it from the database
    public static MealPlanId From(Guid value) => new(value);

    // Overrides ToString to put MealPlanId later in a readable format for database
    public override string ToString() => Value.ToString();
}
