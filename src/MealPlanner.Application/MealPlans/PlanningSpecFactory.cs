using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Catalog;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Planning.DataStructures;
using MealPlanner.Planning.Models;

namespace MealPlanner.Application.MealPlans;

// Converts Domain constraints into a fitting for the Planning problem specification.
// The second and final bridge between Domain and Planning (first is CatalogSnapshotBuilder).
// Returns a result instead of throwing an exception, because conversion can fail because
// of invalid input, which is an expected error, not a system error.
public static class PlanningSpecFactory
{
    public static Result<PlanningSpec> Create(
        PlanningConstraints constraints,
        CatalogSnapshot snapshot,
        IReadOnlyList<int>? excludedDishes = null,
        IReadOnlyDictionary<int, int>? pinnedSlots = null)
    {
        ArgumentNullException.ThrowIfNull(constraints);
        ArgumentNullException.ThrowIfNull(snapshot);

        // Planning represent budget using integer minor units and doesn't know about
        // the currencies. If the budget and catalog used different currencies,
        // comparing their amounts would silently produce incorrect results.
        if (constraints.Budget.Currency != snapshot.Currency)
        {
            return Result.Failure<PlanningSpec>(PlanningErrors.CurrencyMismatch);
        }

        Result<AllergenMask> mask = BuildAllergenMask(constraints.ExcludedAllergens, snapshot);

        if (mask.IsFailure)
        {
            return Result.Failure<PlanningSpec>(mask.Error);
        }

        Result<int[]> disliked = MapIngredients(constraints.DislikedIngredients, snapshot);

        if (disliked.IsFailure)
        {
            return Result.Failure<PlanningSpec>(disliked.Error);
        }

        var budgetMinorUnits = (int)Math.Round(constraints.Budget.Amount * 100m);

        PlanningSpec spec = PlanningSpec.Create(
            budgetMinorUnits,
            constraints.DurationDays,
            [.. constraints.MealsPerDay.Select(CatalogSnapshotBuilder.ToMealKind)],
            (int)constraints.Diet,
            mask.Value,
            disliked.Value,
            constraints.MaxRepeatsPerPlan,
            constraints.MinDaysBetweenRepeats,

            // Missing constraints are represented by neutral values, not nulls,
            // which allows solver's inner loop to make the comparison every time,
            // with the check automatically succeeding if no limit was
            // specified, and the check for null is not needed.
            constraints.MinDailyCalories ?? 0,
            constraints.MaxDailyCalories ?? int.MaxValue,
            constraints.MaxCookingTimeMinutes ?? int.MaxValue,
            [.. constraints.PreferredCuisines.Select(c => (int)c)],
            excludedDishes,
            pinnedSlots);

        return spec;
    }

    // Builds the user's allergen mask.
    private static Result<AllergenMask> BuildAllergenMask(
        IReadOnlyList<AllergenId> excluded,
        CatalogSnapshot snapshot)
    {
        var mask = AllergenMask.Empty;

        foreach (AllergenId id in excluded)
        {
            if (!snapshot.BitByAllergenId.TryGetValue(id, out int position))
            {
                return Result.Failure<AllergenMask>(PlanningErrors.UnknownAllergen);
            }

            mask = mask.With(position);
        }

        return mask;
    }

    // converts disliked ingredients into Planning indices.
    // Unknown identifier is not just ignored and causes a failure.
    // Ignoring it could result in a generated plan that contains
    // user's disliked ingredient without informing the user.
    private static Result<int[]> MapIngredients(
        IReadOnlyList<IngredientId> disliked,
        CatalogSnapshot snapshot)
    {
        var indices = new int[disliked.Count];

        for (int i = 0; i < disliked.Count; i++)
        {
            if (!snapshot.IndexByIngredientId.TryGetValue(disliked[i], out int index))
            {
                return Result.Failure<int[]>(PlanningErrors.UnknownIngredient);
            }

            indices[i] = index;
        }

        return indices;
    }
}
