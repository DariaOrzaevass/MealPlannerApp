using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;

namespace MealPlanner.Planning.Models;

// A snapshot of dishes in the system that will not change during the planning of the meal plan
public sealed class DishCatalog
{
    internal DishCatalog(
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
