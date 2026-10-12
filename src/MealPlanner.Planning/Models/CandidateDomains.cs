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

        // Dishes rejected by user that need to be regenerated
        int rejectedByExclusion = 0;

        foreach (int excluded in spec.ExcludedDishes)
        {
            // DishId is not negative and within the catalog and is not forbidden before
            if (excluded >= 0 && excluded < dishCount && !forbidden[excluded])
            {
                forbidden[excluded] = true;
                rejectedByExclusion++;
            }
        }

        int rejectedByAllergens = 0;
        int rejectedByDiet = 0;
        int rejectedByCookingTime = 0;

        // Group dishes by meal kind
        var buckets = new List<int>?[MealKindIndex.Count];
        var minCost = new int[MealKindIndex.Count];

        // Total cost of valid dishes by meal kind
        var costSum = new long[MealKindIndex.Count];
        var minCalories = new int[MealKindIndex.Count];
        var maxCalories = new int[MealKindIndex.Count];
        Array.Fill(minCost, int.MaxValue);
        Array.Fill(minCalories, int.MaxValue);

        // Which meal kinds are requested in this problem
        var requestedKinds = new bool[MealKindIndex.Count];

        foreach (MealKinds kind in spec.MealsPerDay)
        {
            int kindIndex = MealKindIndex.ToIndex(kind);
            requestedKinds[kindIndex] = true;

            // Creates a list where valid dish ids of this meal kind will
            // be stored divide by 4 because Breakfast, Lunch, Dinner, Snack
            buckets[kindIndex] = new List<int>(dishCount / 4);
        }

        // Copies often used catalog arrays. Can help performance
        AllergenMask[] allergens = catalog.Allergens;
        int[] dietLevels = catalog.DietLevels;
        int[] cookingTime = catalog.CookingTimeMinutes;
        MealKinds[] suitableForAll = catalog.SuitableFor;
        int[] costs = catalog.CostMinorUnits;
        int[] calorieValues = catalog.Calories;

        // Users constraints are also copied
        AllergenMask excludedAllergens = spec.ExcludedAllergens;
        int userDietLevel = spec.DietLevel;
        int maxCookingTime = spec.MaxCookingTimeMinutes;

        // Goes through the dish catalog and removes dishes violating constraints
        for (int dish = 0; dish < dishCount; dish++)
        {
            // If the dish already forbidden do not check it anymore
            if (forbidden[dish])
            {
                continue;
            }

            // If found excluded allergen - forbid the dish and increase rejectedByAllergens by 1
            if (allergens[dish].Intersects(excludedAllergens))
            {
                rejectedByAllergens++;

                continue;
            }

            // The users diet level is not more than dish diet level.
            // Vegan user (2) cannot eat vegetarian dish (1)
            if (dietLevels[dish] < userDietLevel)
            {
                rejectedByDiet++;

                continue;
            }

            // Cooking time is not more than the maximum allowed
            if (cookingTime[dish] > maxCookingTime)
            {
                rejectedByCookingTime++;

                continue;
            }

            // Which meal kinds does this dish fits
            MealKinds suitable = suitableForAll[dish];
            int cost = costs[dish];
            int calories = calorieValues[dish];

            // Checks all the meal kinds
            for (int kindIndex = 0; kindIndex < MealKindIndex.Count; kindIndex++)
            {
                // If user does not requested this meal kind just ignore it
                if (!requestedKinds[kindIndex])
                {
                    continue;
                }

                // If the dish is suitable for a particular meal kind (bitmasks and biwise operations)
                if ((suitable & MealKindIndex.FromIndex(kindIndex)) == 0)
                {
                    continue;
                }

                // Adds the dish to the list and its cost to the total sum
                buckets[kindIndex]!.Add(dish);
                costSum[kindIndex] += cost;

                // Updates the cheapest dish
                if (cost < minCost[kindIndex])
                {
                    minCost[kindIndex] = cost;
                }
                // Updates the minimum calorie dish
                if (calories < minCalories[kindIndex])
                {
                    minCalories[kindIndex] = calories;
                }
                // Updates the maximum calorie dish
                if (calories > maxCalories[kindIndex])
                {
                    maxCalories[kindIndex] = calories;
                }
            }
        }

        var byKind = new int[]?[MealKindIndex.Count];
        var averageCost = new double[MealKindIndex.Count];
        bool hasEmpty = false;

        // Checks all the meal kinds
        for (int kindIndex = 0; kindIndex < MealKindIndex.Count; kindIndex++)
        {
            if (!requestedKinds[kindIndex])
            {
                continue;
            }

            byKind[kindIndex] = [.. buckets[kindIndex]!];

            // Is requested meal kind have 0 valid dishes?
            if (byKind[kindIndex]!.Length == 0)
            {
                hasEmpty = true;
                minCost[kindIndex] = 0;
                averageCost[kindIndex] = 0;
                minCalories[kindIndex] = 0;
            }
            else
            {
                // Counts average cost for the meal kind dishes
                averageCost[kindIndex] = (double)costSum[kindIndex] / byKind[kindIndex]!.Length;
            }
        }

        // Array with pinned dishes
        var pinnedDish = new int[spec.SlotCount];
        // Are with pinned dishes feasible
        var pinnedFeasible = new bool[spec.SlotCount];

        // Goes through all the slots in the meal plan
        for (int slot = 0; slot < spec.SlotCount; slot++)
        {
            // Gets the pinned dish for the slot
            int dish = spec.PinnedSlots[slot];
            pinnedDish[slot] = dish;

            // If dish is not pinned it is -1
            if (dish < 0)
            {
                continue;
            }

            // If the dish is pinned to the slot, which meal kind is that slot represented by index
            int kindIndex = MealKindIndex.ToIndex(spec.Slots[slot].Kind);
            // Is the dish among the allowed dishes array (if it exist or empty if don't)?
            // If it appers in the array then Array.IndexOf will return the index
            pinnedFeasible[slot] = Array.IndexOf(byKind[kindIndex] ?? [], dish) >= 0;

            // Invalid dish is pinned
            if (!pinnedFeasible[slot])
            {
                hasEmpty = true;
            }
        }

        // Creates DomainBuildStats - statistic of regected dishes
        var stats = new DomainBuildStats(
            dishCount,
            rejectedByIngredients,
            rejectedByAllergens,
            rejectedByDiet,
            rejectedByCookingTime,
            rejectedByExclusion);

        // Returns new Candidate Domains
        return new CandidateDomains(
            byKind, minCost, averageCost, minCalories, maxCalories,
            pinnedDish, pinnedFeasible, stats)
        {
            HasEmptyDomain = hasEmpty
        };
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
