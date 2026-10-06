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

    // Returns an error if the meal type has been repeated. For example, cannot have two Breakfasts in one day
    public static readonly Error DuplicateMealType =
        Error.Validation("MealPlan.DuplicateMealType", "The meal type has been repeated.");

    // Returns an error if the the maximum amount of dish repeats is negative - the dish cannot repeat in plan -1 times
    public static readonly Error NonPositiveMaxRepeats =
        Error.Validation("MealPlan.NonPositiveMaxRepeats", "The dish must be appear in meal plan at least once.");

    // Returns an error if the days between repeats are negative
    public static readonly Error NegativeRepeatGap =
        Error.Validation("MealPlan.NegativeRepeatGap", "Days between repeats of the dish cannot be negative.");

    // Returns an error if the maximum amount of calories is less than a minimum
    public static readonly Error InvertedCalorieRange =
        Error.Validation("MealPlan.InvertedCalorieRange", "Maximum amount of calories must be more than a minimum amount.");

    // Returns an error if the minimum cooking time is negative
    public static readonly Error NonPositiveCookingTimeLimit =
       Error.Validation("MealPlan.NonPositiveCookingTimeLimit", "Cooking time must be positive.");
}
