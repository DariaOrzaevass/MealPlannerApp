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

    // Returns an error if the name of the meal plan is empty
    public static readonly Error EmptyName =
        Error.Validation("MealPlan.EmptyName", "The name of the plan cannot be empty");

    // Returns an error if there is a mismatch between plan items and slots available
    public static readonly Error SlotCountMismatch =
        Error.Problem("MealPlan.SlotCountMismatch", "The number of plan items does not match the number of slots.");

    // returns an error if a second plan item is generated for the same slot
    public static readonly Error DuplicateSlot =
        Error.Problem("MealPlan.DuplicateSlot", "Two plan items are generated for the same slot.");

    // Returns an error if the plan item is created for the slot outside of the duration of the meal plan
    public static readonly Error SlotOutOfRange =
        Error.Problem("MealPlan.SlotOutOfRange", "The plan item does not match the duration of the plan.");

    // Returns an error if there is the entry is not found
    public static readonly Error EntryNotFound =
        Error.NotFound("MealPlan.EntryNotFound", "The entry is not found in the meal plan.");

    // Returns an error if the locked dish was changed in meal regeneration
    public static readonly Error LockedEntryChanged =
       Error.Failure(
           "MealPlan.LockedEntryChanged", "Regenerated plan changed the locked entry.");

    // Returns an error if something is trying to change a locked entry
    public static readonly Error EntryLocked =
        Error.Conflict("MealPlan.EntryLocked", "The entry was locked and cannot be regenerated.");

    // Returns an error if the total cost exceeds the budget after regeneration
    public static readonly Error BudgetExceeded =
        Error.Problem("MealPlan.BudgetExceeded", "After regeneration the total cost exceeds the budget.");

    // Returns an error if after the regeneration the same dish is returned
    public static readonly Error SameDish =
        Error.Problem("MealPlan.SameDish", "The regenerated dish is the same with the previous one.");
}
