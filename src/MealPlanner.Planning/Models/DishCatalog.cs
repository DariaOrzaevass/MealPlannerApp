using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;

namespace MealPlanner.Planning.Models;

// A snapshot of dishes in the system that will not change during the planning of the meal plan
public sealed class DishCatalog
{
    // Public method create that checks the offsets and that
    // each dish's ingredient ids are sorted and unique.
    public static DishCatalog Create(
        int[] costMinorUnits,
        int[] calories,
        MealKinds[] suitableFor,
        AllergenMask[] allergens,
        int[] cookingTimeMinutes,
        int[] dietLevels,
        int[] cuisineIds,
        int[] ingredientOffsets,
        int[] ingredientIds,
        int ingredientCount)
    {
        // Throw exception if any of the required arrays
        // are null and ingredientCount is negative
        ArgumentNullException.ThrowIfNull(costMinorUnits);
        ArgumentNullException.ThrowIfNull(calories);
        ArgumentNullException.ThrowIfNull(suitableFor);
        ArgumentNullException.ThrowIfNull(allergens);
        ArgumentNullException.ThrowIfNull(cookingTimeMinutes);
        ArgumentNullException.ThrowIfNull(dietLevels);
        ArgumentNullException.ThrowIfNull(cuisineIds);
        ArgumentNullException.ThrowIfNull(ingredientOffsets);
        ArgumentNullException.ThrowIfNull(ingredientIds);
        ArgumentOutOfRangeException.ThrowIfNegative(ingredientCount);

        // Number of dishes
        int count = costMinorUnits.Length;

        // Checks that every dish has required properties
        if (calories.Length != count
            || suitableFor.Length != count
            || allergens.Length != count
            || cookingTimeMinutes.Length != count
            || dietLevels.Length != count
            || cuisineIds.Length != count)
        {
            throw new ArgumentException(
                "Arrays of properties of the meal catalogue must be the same length.");
        }

        // ingredientOffsets array nedds one more value to mark the end on the last dish
        if (ingredientOffsets.Length != count + 1)
        {
            throw new ArgumentException(
                "The length of ingredientOffsets array must be more than the number of dishes by 1.",
                nameof(ingredientOffsets));
        }

        // Checks the validity of offsets
        if (ingredientOffsets[0] != 0 || ingredientOffsets[count] != ingredientIds.Length)
        {
            throw new ArgumentException(
                "The offsets must begin from 0 and and end after the last ingredient.",
                nameof(ingredientOffsets));
        }

        // Dish validation
        for (int dish = 0; dish < count; dish++)
        {
            int start = ingredientOffsets[dish];
            int end = ingredientOffsets[dish + 1];

            if (end < start)
            {
                throw new ArgumentException(
                    "Ingredient offsets must not decrease.",
                    nameof(ingredientOffsets));
            }

            // int.MinValue - the smallest value of integer for
            // easier comparison beginning with first ingredient
            int previous = int.MinValue;

            for (int index = start; index < end; index++)
            {
                int ingredientId = ingredientIds[index];

                if ((uint)ingredientId >= (uint)ingredientCount || ingredientId <= previous)
                {
                    throw new ArgumentException(
                        "Ingredients' IDs of the dish must be sorted in the ascending order" +
                        "and must be inside a valid range of ingredients' IDs.",
                        nameof(ingredientIds));
                }

                previous = ingredientId;
            }
        }

        // Creates a new DishCatalog
        return new DishCatalog(
            costMinorUnits,
            calories,
            suitableFor,
            allergens,
            cookingTimeMinutes,
            dietLevels,
            cuisineIds,
            ingredientOffsets,
            ingredientIds,
            ingredientCount);
    }

    private DishCatalog(
        int[] costMinorUnits,
        int[] calories,
        MealKinds[] suitableFor,
        AllergenMask[] allergens,
        int[] cookingTimeMinutes,
        int[] dietLevels,
        int[] cuisineIds,
        int[] ingredientOffsets,
        int[] ingredientIds,
        int ingredientCount)
    {
        CostMinorUnits = costMinorUnits;
        Calories = calories;
        SuitableFor = suitableFor;
        Allergens = allergens;
        CookingTimeMinutes = cookingTimeMinutes;
        DietLevels = dietLevels;
        CuisineIds = cuisineIds;
        IngredientOffsets = ingredientOffsets;
        IngredientIds = ingredientIds;
        IngredientCount = ingredientCount;
    }

    // How many dishes there are in the catalog
    public int Count => CostMinorUnits.Length;

    // How many ingredients there are in the catalog
    public int IngredientCount { get; }

    // Cost in the minor units of the currency(cents). Is made int for the speed
    // of the calculations(decimal is longer), if using minor units - no mistakes
    public int[] CostMinorUnits { get; }

    // Calories of one portion of the dish (in kcal)
    public int[] Calories { get; }

    // Kinds of meal to which the dishes could be suitable
    public MealKinds[] SuitableFor { get; }

    // The allergen masks of dishes
    public AllergenMask[] Allergens { get; }

    // The coocking time of the dish
    public int[] CookingTimeMinutes { get; }

    // 0 - Omnivore(no limitations), 1 - Vegetarian, 2 - Vegan
    public int[] DietLevels { get; }
    
    // Id off the cuisine. Is used in checking similarities of dishes
    public int[] CuisineIds { get; }

    // Boundaries of the dish ingredients. Ingredients of dish d are from
    // IngredientOffsets[d] to IngredientOffsets[d + 1], excluding the latter.
    // The length is Count + 1 so that we have an offset to end the last dish.
    public int[] IngredientOffsets { get; }

    // Array of all ingredients of all dishes one after each other
    public int[] IngredientIds { get; }

    // Show ingredients of the dish without copying and allocating new memory
    public ReadOnlySpan<int> IngredientsOf(int dish)
    {
        int start = IngredientOffsets[dish];

        return IngredientIds.AsSpan(start, IngredientOffsets[dish + 1] - start);
    }

    // How many ingredients are in the dish
    public int IngredientCountOf(int dish) => IngredientOffsets[dish + 1] - IngredientOffsets[dish];

    // The cheapest dish in the catalog in minor units. For lower bound estimates
    // (can the cheapest dish make a meal plan satisfying budget constraint at that moment)
    public int MinCostMinorUnits() => CostMinorUnits.Length == 0 ? 0 : CostMinorUnits.Min();

}
