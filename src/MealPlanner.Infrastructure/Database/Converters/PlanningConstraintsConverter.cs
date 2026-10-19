using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Shared;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MealPlanner.Infrastructure.Database.Converters;

// Converter for the PlanningConstraints for the Entity
// Framework Core to store them as a JSON string
internal sealed class PlanningConstraintsConverter()
    : ValueConverter<PlanningConstraints, string>(
        constraints => Serialize(constraints),
        json => Deserialize(json))
{
    // Creates serializer settings using Web defaults
#pragma warning disable IDE1006 // Naming Styles
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
#pragma warning restore IDE1006 // Naming Styles

    // Database storage format, independent of domain types
    private sealed record Snapshot(
        decimal BudgetAmount,
        string BudgetCurrency,
        int DurationDays,
        int[] MealsPerDay,
        int Diet,
        Guid[] ExcludedAllergens,
        Guid[] DislikedIngredients,
        int MaxRepeatsPerPlan,
        int MinDaysBetweenRepeats,
        int? MinDailyCalories,
        int? MaxDailyCalories,
        int? MaxCookingTimeMinutes,
        int[]? PreferredCuisines);

    // Converts PlanningConstraints object into Snapshot and then serializes it
    private static string Serialize(PlanningConstraints constraints) =>
    JsonSerializer.Serialize(
        new Snapshot(
            constraints.Budget.Amount,
            constraints.Budget.Currency.Code,
            constraints.DurationDays,
            [.. constraints.MealsPerDay.Select(m => (int)m)],
            (int)constraints.Diet,
            [.. constraints.ExcludedAllergens.Select(a => a.Value)],
            [.. constraints.DislikedIngredients.Select(i => i.Value)],
            constraints.MaxRepeatsPerPlan,
            constraints.MinDaysBetweenRepeats,
            constraints.MinDailyCalories,
            constraints.MaxDailyCalories,
            constraints.MaxCookingTimeMinutes,
            [.. constraints.PreferredCuisines.Select(c => (int)c)]),
        Options);

    // Deserializes JSON string into PlanningConstraints through Snapshot
    private static PlanningConstraints Deserialize(string json)
    {
        // Throws an exception if Deserialization returns null
        Snapshot snapshot = JsonSerializer.Deserialize<Snapshot>(json, Options)
            ?? throw new InvalidOperationException("Could not read the Planning Constraints.");

        // Reconstructs PlanningConstraints from the Snapshot
        return PlanningConstraints.Create(
            new Money(snapshot.BudgetAmount, Currency.FromCode(snapshot.BudgetCurrency)),
            snapshot.DurationDays,
            [.. snapshot.MealsPerDay.Select(m => (MealType)m)],
            (DietType)snapshot.Diet,
            [.. snapshot.ExcludedAllergens.Select(AllergenId.From)],
            [.. snapshot.DislikedIngredients.Select(IngredientId.From)],
            snapshot.MaxRepeatsPerPlan,
            snapshot.MinDaysBetweenRepeats,
            snapshot.MinDailyCalories,
            snapshot.MaxDailyCalories,
            snapshot.MaxCookingTimeMinutes,
            [.. (snapshot.PreferredCuisines ?? []).Select(c => (Cuisine)c)]).Value;
    }

    // Value comparison for reliable recognition of changes
    // on every access. Compares serialized JSON to detect changes
    public static ValueComparer<PlanningConstraints> Comparer { get; } = new(
    (left, right) => Serialize(left!) == Serialize(right!),
    constraints => Serialize(constraints).GetHashCode(StringComparison.Ordinal),
    constraints => Deserialize(Serialize(constraints)));
}
