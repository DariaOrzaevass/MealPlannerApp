using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Application.MealPlans;

// Validates the Planning Constraints. One validator for both generation of meal plans and comparison of solvers
internal sealed class PlanningConstraintsInputValidator : AbstractValidator<PlanningConstraintsInput>
{
    public PlanningConstraintsInputValidator()
    {
        RuleFor(c => c.Budget)
            .GreaterThan(0).WithErrorCode("MealPlan.NonPositiveBudget")
            .WithMessage("Budget must be greater than zero.");

        RuleFor(c => c.CurrencyCode)
            .Must(code => Currency.TryFromCode(code) is not null)
            .WithErrorCode("MealPlan.UnknownCurrency")
            .WithMessage("Unknown currency code.");

        RuleFor(c => c.DurationDays)
            .InclusiveBetween(1, PlanningConstraints.MaxDurationDays)
            .WithErrorCode("MealPlan.DurationOutOfRange")
            .WithMessage($"Plan duration must be from 1 to {PlanningConstraints.MaxDurationDays} days.");

        RuleFor(c => c.MealsPerDay)
            .NotEmpty().WithErrorCode("MealPlan.NoMealsSelected")
            .WithMessage("Select at least one meal type.")
            .Must(meals => meals.Distinct().Count() == meals.Count)
            .WithErrorCode("MealPlan.DuplicateMealType")
            .WithMessage("Meal types must not repeat.");

        // Every item in the list is validated separately. RuleFor would validate
        // the whole collection, but it is importsnt here to find an invalid item.
        RuleForEach(c => c.MealsPerDay)
            .IsInEnum().WithErrorCode("MealPlan.UnknownMealType")
            .WithMessage("Unknown meal type.");

        RuleFor(c => c.Diet)
            .IsInEnum().WithErrorCode("MealPlan.UnknownDiet")
            .WithMessage("Unknown diet type.");

        RuleFor(c => c.MaxRepeatsPerPlan)
            .GreaterThanOrEqualTo(1).WithErrorCode("MealPlan.NonPositiveMaxRepeats")
            .WithMessage("Dishes must be allowed to appear at least once.");

        RuleFor(c => c.MinDaysBetweenRepeats)
            .GreaterThanOrEqualTo(0).WithErrorCode("MealPlan.NegativeRepeatGap")
            .WithMessage("The gap between dish repetitions cannot be negative.");

        // The condition of both limits being specified is checked here
        // and not in each rule, because having one limit is also valid 
        RuleFor(c => c.MinDailyCalories)
            .LessThanOrEqualTo(c => c.MaxDailyCalories)
            .When(c => c.MinDailyCalories is not null && c.MaxDailyCalories is not null)
            .WithErrorCode("MealPlan.InvertedCalorieRange")
            .WithMessage("The minimum calorie count limit cannot be bigger than the maximum.");

        RuleFor(c => c.MaxCookingTimeMinutes)
            .GreaterThan(0).When(c => c.MaxCookingTimeMinutes is not null)
            .WithErrorCode("MealPlan.NonPositiveCookingTimeLimit")
            .WithMessage("The cooking time limit must be greater than zero.");

        // An empty Guid is most definetely an indication of client-side value,
        // not the user's actual choice, so it should be reported explicitly.
        RuleFor(c => c.ExcludedAllergens)
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithErrorCode("MealPlan.EmptyAllergenId")
            .WithMessage("Allergen identifier cannot be empty.");

        RuleFor(c => c.DislikedIngredients)
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithErrorCode("MealPlan.EmptyIngredientId")
            .WithMessage("Ingredient identifier cannot be empty.");
    }
}
