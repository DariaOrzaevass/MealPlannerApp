using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Domain.MealPlans;

// Class containing all errors related to meal plans
public static class MealPlanErrors
{
    // Returns an error if the meal plan with this ID is not found
    public static Error NotFound(MealPlanId id) =>
        Error.NotFound("MealPlan.NotFound", $"Meal Plan with ID {id} not found.");

    // Returns an error if the budget is negative
    public static readonly Error NonPositiveBudget =
        Error.Validation("MealPlan.NonPositiveBudget", "Budget must be positive.");

    // Returns an error if the duration of the meal plan is less than 1 or more than MaxDurationDays
    public static readonly Error DurationOutOfRange =
        Error.Validation(
            "MealPlan.DurationOutOfRange",
            $"Duration of a meal plan must be between 1 and {PlanningConstraints.MaxDurationDays} days");

    // Returns an error if the user does not have any meals per day
    public static readonly Error NoMealsSelected =
        Error.Validation("MealPlan.NoMealsSelected", "Must have at least one meal per day");
}
