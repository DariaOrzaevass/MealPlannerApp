using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.MealPlans;

// A Meal Plan entity representing a specific Meal Plan
public sealed class MealPlan : Entity<MealPlanId>
{
    // The list of all the items in the meal plan
    private readonly List<MealPlanEntry> _entries = [];

    // Default constructor for EF core to get the Meal Plan from the database
    private MealPlan()
    {
    }
}
