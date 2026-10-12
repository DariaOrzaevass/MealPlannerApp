using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;

namespace MealPlanner.Planning.Models;

// Planning problem in terms used in the Planning layer
public sealed class PlanningSpec
{
    // Private constructor so that the PlanningSpec can only be created using the Create method with validation
    private PlanningSpec(
    int budgetMinorUnits,
    int durationDays,
    MealKinds[] mealsPerDay,
    int dietLevel,
    AllergenMask excludedAllergens,
    int[] dislikedIngredients,
    int maxRepeats,
    int minDaysBetweenRepeats,
    int minDailyCalories,
    int maxDailyCalories,
    int maxCookingTimeMinutes,
    int[] preferredCuisines,
    int[] excludedDishes,
    int[] pinnedSlots,
    SlotDescriptor[] slots)
    {
        BudgetMinorUnits = budgetMinorUnits;
        DurationDays = durationDays;
        MealsPerDay = mealsPerDay;
        DietLevel = dietLevel;
        ExcludedAllergens = excludedAllergens;
        DislikedIngredients = dislikedIngredients;
        MaxRepeats = maxRepeats;
        MinDaysBetweenRepeats = minDaysBetweenRepeats;
        MinDailyCalories = minDailyCalories;
        MaxDailyCalories = maxDailyCalories;
        MaxCookingTimeMinutes = maxCookingTimeMinutes;
        PreferredCuisines = preferredCuisines;
        ExcludedDishes = excludedDishes;
        PinnedSlots = pinnedSlots;
        Slots = slots;
    }

    // Budget constraint in minor units (cents)
    public int BudgetMinorUnits { get; }

    // Duration of the meal plan
    public int DurationDays { get; }

    // Meal kinds that should appear each day
    public IReadOnlyList<MealKinds> MealsPerDay { get; }

    // 0 - Omnivore(no limitations), 1 - Vegetarian, 2 - Vegan
    public int DietLevel { get; }

    // Allergens of the user
    public AllergenMask ExcludedAllergens { get; }

    // Disliked ingredients of the user
    public IReadOnlyList<int> DislikedIngredients { get; }

    // Maximum alloud number of repeats of each dish
    public int MaxRepeats { get; }

    // Minimum number of days between repeats of the dish
    public int MinDaysBetweenRepeats { get; }

    // Minimum amount of calories per day. 0 if no contraints here
    public int MinDailyCalories { get; }

    // Maximum amount of calories per day. MaxValue if no contraints here
    public int MaxDailyCalories { get; }

    // Maximum allowed cooking time in minutes. MaxValue if no contraints here
    public int MaxCookingTimeMinutes { get; }

    // Preffered cuisines of the user. If empty - no preferences
    public IReadOnlyList<int> PreferredCuisines { get; }

    // Dishes that must not be suggested. For example for regeneration of meal plan without them
    public IReadOnlyList<int> ExcludedDishes { get; }

    // Slots with pinned to them dishes that do not change. -1 if no dish pinned
    public IReadOnlyList<int> PinnedSlots { get; }

    // Does this slot has a pinned dish
    public bool IsPinned(int slotIndex) => PinnedSlots[slotIndex] >= 0;

    // All slots of the 1 day, then 2, then 3...
    public IReadOnlyList<SlotDescriptor> Slots { get; }

    // The number of slots
    public int SlotCount => Slots.Count;

    // Creates a PlanningSpec after validation
    public static PlanningSpec Create(
    int budgetMinorUnits,
    int durationDays,
    IReadOnlyList<MealKinds> mealsPerDay,
    int dietLevel = 0,
    AllergenMask excludedAllergens = default,
    IReadOnlyList<int>? dislikedIngredients = null,
    int maxRepeats = int.MaxValue,
    int minDaysBetweenRepeats = 0,
    int minDailyCalories = 0,
    int maxDailyCalories = int.MaxValue,
    int maxCookingTimeMinutes = int.MaxValue,
    IReadOnlyList<int>? preferredCuisines = null,
    IReadOnlyList<int>? excludedDishes = null,
    IReadOnlyDictionary<int, int>? pinnedSlots = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(budgetMinorUnits);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationDays);
        ArgumentNullException.ThrowIfNull(mealsPerDay);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(mealsPerDay.Count);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRepeats);
        ArgumentOutOfRangeException.ThrowIfNegative(minDaysBetweenRepeats);

        foreach (MealKinds kind in mealsPerDay)
        {
            if (!MealKindIndex.IsSingleKind(kind))
            {
                throw new ArgumentException(
                    "The slot is defined by only one Meal Kind, not a combination.",
                    nameof(mealsPerDay));
            }
        }

        // Creates an array to hold all meal plan slots
        var slots = new SlotDescriptor[durationDays * mealsPerDay.Count];
        int index = 0;

        // Goes through each days each meal kind and creates actual slots
        for (int day = 1; day <= durationDays; day++)
        {
            foreach (MealKinds kind in mealsPerDay)
            {
                slots[index] = new SlotDescriptor(index, day, kind);
                index++;
            }
        }

        // Returns a new array of disliked ingredients with no duplicates in ascending order
        int[] disliked = dislikedIngredients is null
            ? []
            : [.. dislikedIngredients.Distinct().Order()];

        // Creates pinned array with the number of elements equal to the number of slots
        var pinned = new int[slots.Length];

        // Array indexing is useful for the solvers
        // Fills pinned with -1
        Array.Fill(pinned, -1);

        // Checks that there are pinned slots 
        if (pinnedSlots is not null)
        {
            // Goes through the pairs of slot - pinned dish
            foreach ((int slot, int dish) in pinnedSlots)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(slot);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, slots.Length);

                // Stores the pinned dish id at that slot
                pinned[slot] = dish;
            }
        }

        // Returns new PlanningSpec
        return new PlanningSpec(
            budgetMinorUnits,
            durationDays,
            [.. mealsPerDay],
            dietLevel,
            excludedAllergens,
            disliked,
            maxRepeats,
            minDaysBetweenRepeats,
            minDailyCalories,
            maxDailyCalories,
            maxCookingTimeMinutes,
            preferredCuisines is null ? [] : [.. preferredCuisines.Distinct().Order()],
            excludedDishes is null ? [] : [.. excludedDishes.Distinct().Order()],
            pinned,
            slots);
    }
}
