using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Application.MealPlans;

// The planning constraints in the form received from client.
// Needed for both meal plan generation and comparison.
// Primitive values are used instead of domain types, because
// in this form they arrive from the UI. Converting
// into PlanningConstraints already does the domain validation.
public sealed record PlanningConstraintsInput(
    decimal Budget,
    string CurrencyCode,
    int DurationDays,
    IReadOnlyList<MealType> MealsPerDay,
    DietType Diet,
    IReadOnlyList<Guid> ExcludedAllergens,
    IReadOnlyList<Guid> DislikedIngredients,
    int MaxRepeatsPerPlan,
    int MinDaysBetweenRepeats,
    int? MinDailyCalories,
    int? MaxDailyCalories,
    int? MaxCookingTimeMinutes,
    IReadOnlyList<Cuisine> PreferredCuisines)
{
    // Creates domain constraints. Their creation also validates domain rules.
    public Result<PlanningConstraints> ToConstraints()
    {
        // Currency code comes from request, so it is parsed without
        // errors. Unknown code = input error, not application failure.
        Currency? currency = Currency.TryFromCode(CurrencyCode);

        if (currency is null)
        {
            return Result.Failure<PlanningConstraints>(PlanningErrors.UnknownCurrency);
        }

        return PlanningConstraints.Create(
            new Money(Budget, currency),
            DurationDays,
            MealsPerDay,
            Diet,
            [.. ExcludedAllergens.Select(AllergenId.From)],
            [.. DislikedIngredients.Select(IngredientId.From)],
            MaxRepeatsPerPlan,
            MinDaysBetweenRepeats,
            MinDailyCalories,
            MaxDailyCalories,
            MaxCookingTimeMinutes,
            PreferredCuisines);
    }
}
