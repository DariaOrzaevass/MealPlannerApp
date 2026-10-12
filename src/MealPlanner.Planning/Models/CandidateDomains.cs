using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;

namespace MealPlanner.Planning.Models;

// Contains all possible dishes considering all filters (constraints)
public sealed class CandidateDomains
{
    private readonly int[]?[] _byKind;
    private readonly int[] _minCostByKind;
    private readonly double[] _averageCostByKind;
    private readonly int[] _minCaloriesByKind;
    private readonly int[] _maxCaloriesByKind;

    // Stores exactly one allowed dish for each pinned slot.
    // Slotes with no pinned dishes are just not here
    private readonly int[] _pinnedDish;

    private readonly bool[] _pinnedFeasible;

    // Private so CandidateDomains can only be built inside the class
    private CandidateDomains(
    int[]?[] byKind,
    int[] minCostByKind,
    double[] averageCostByKind,
    int[] minCaloriesByKind,
    int[] maxCaloriesByKind,
    int[] pinnedDish,
    bool[] pinnedFeasible,
    DomainBuildStats stats)
    {
        _pinnedDish = pinnedDish;
        _pinnedFeasible = pinnedFeasible;
        _byKind = byKind;
        _minCostByKind = minCostByKind;
        _averageCostByKind = averageCostByKind;
        _minCaloriesByKind = minCaloriesByKind;
        _maxCaloriesByKind = maxCaloriesByKind;
        Stats = stats;
    }

    // How many dishes were removed by each filter. Can be useful for future analysis
    public DomainBuildStats Stats { get; }

    // Are there a meal kind with no dishes
    public bool HasEmptyDomain { get; private init; }

    // Returns valid dishes for this meal kind if there are any or empty collection
    public ReadOnlySpan<int> ForKind(MealKinds kind) =>
    _byKind[MealKindIndex.ToIndex(kind)] ?? [];

    // Returns valid dishes for the slot
    public ReadOnlySpan<int> ForSlot(SlotDescriptor slot)
    {
        // If dish is not pinned here ForKind is returned for this meal kind
        if (_pinnedDish[slot.Index] < 0)
        {
            return ForKind(slot.Kind);
        }

        // If dish is pinned but is not valid anymore, the empty collection will be returned
        return _pinnedFeasible[slot.Index] ? _pinnedDish.AsSpan(slot.Index, 1) : [];
    }

    // How many valid dishes there are for this meal kind
    public int SizeForKind(MealKinds kind) => ForKind(kind).Length;

    // Returns the price of the cheapest available dish for this meal kind for the lower bound
    public int MinCostForKind(MealKinds kind) => _minCostByKind[MealKindIndex.ToIndex(kind)];

    // Returns the average price of the available dishes for this meal kind. For proportional allocation
    // of the budget, because average breakfast cost can be lower than average dinner cost for example
    public double AverageCostForKind(MealKinds kind) => _averageCostByKind[MealKindIndex.ToIndex(kind)];

    // Returns calories of the lowest calorie valid dish for this kind
    public int MinCaloriesForKind(MealKinds kind) => _minCaloriesByKind[MealKindIndex.ToIndex(kind)];

    // Returns calories of the highest calorie valid dish for this kind
    public int MaxCaloriesForKind(MealKinds kind) => _maxCaloriesByKind[MealKindIndex.ToIndex(kind)];

    public static CandidateDomains Build(DishCatalog catalog, DishIndex index, PlanningSpec spec)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(spec);

        int dishCount = catalog.Count;

        // Tells which dishes are forbidden and how many are there dishes like that
        var forbidden = new bool[dishCount];
        int rejectedByIngredients = index.MarkDishesWithAny(
            [.. spec.DislikedIngredients],
            forbidden);


    }
}

// How many dishes were removed by each filter. Shows how the search space is reduced before the search
// Can explain why the plan cannot be generated, suggesting user that a constraint should maybe be eased
public readonly record struct DomainBuildStats(
    int TotalDishes,
    int RejectedByIngredients,
    int RejectedByAllergens,
    int RejectedByDiet,
    int RejectedByCookingTime,
    int RejectedByExclusion = 0)
{
    // How many dishes "survived" all filters
    public int Survived =>
    TotalDishes
    - RejectedByIngredients
    - RejectedByAllergens
    - RejectedByDiet
    - RejectedByCookingTime
    - RejectedByExclusion;

    // The part of the dish catalog that "survived" from 0 to 1
    public double SurvivalRate => TotalDishes == 0 ? 0 : (double)Survived / TotalDishes;
}
