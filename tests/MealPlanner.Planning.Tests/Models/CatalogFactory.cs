using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Tests.Models;

// Test helper class for creating a sample dish catalog
internal static class CatalogFactory
{
    // Creates a dish with default test values
    public static DishDescriptor Dish(
        int costMinorUnits = 12_000,
        int calories = 350,
        MealKinds suitableFor = MealKinds.Lunch | MealKinds.Dinner,
        int cookingTimeMinutes = 40,
        int dietLevel = 0,
        int cuisineId = 0,
        IReadOnlyList<int>? ingredientIds = null,
        IReadOnlyList<int>? allergenPositions = null) =>
        new(
            costMinorUnits,
            calories,
            suitableFor,
            cookingTimeMinutes,
            dietLevel,
            cuisineId,
            ingredientIds ?? [0, 1],
            allergenPositions ?? []);

    // Creates a sample catalog of 6 dishes with intentionally
    // different properties to test various constraints
    public static DishCatalog Sample() =>
        DishCatalogBuilder.Build(
            [
                Dish(costMinorUnits: 4_000, calories: 300, suitableFor: MealKinds.Breakfast,
                    cookingTimeMinutes: 10, dietLevel: 2, cuisineId: 1,
                    ingredientIds: [0, 1], allergenPositions: [2]),

                Dish(costMinorUnits: 9_000, calories: 420, suitableFor: MealKinds.Breakfast | MealKinds.Dinner,
                        cookingTimeMinutes: 15, dietLevel: 1, cuisineId: 2,
                        ingredientIds: [2, 3], allergenPositions: [0, 1]),

                Dish(costMinorUnits: 12_000, calories: 350, suitableFor: MealKinds.Lunch | MealKinds.Dinner,
                        cookingTimeMinutes: 60, dietLevel: 1, cuisineId: 1,
                        ingredientIds: [4, 5, 6], allergenPositions: []),

                Dish(costMinorUnits: 25_000, calories: 700, suitableFor: MealKinds.Lunch,
                        cookingTimeMinutes: 90, dietLevel: 0, cuisineId: 3,
                        ingredientIds: [7, 8, 9], allergenPositions: []),

                Dish(costMinorUnits: 15_000, calories: 550, suitableFor: MealKinds.Lunch | MealKinds.Dinner,
                        cookingTimeMinutes: 25, dietLevel: 1, cuisineId: 2,
                        ingredientIds: [10, 11], allergenPositions: [2, 3]),

                Dish(costMinorUnits: 7_000, calories: 180, suitableFor: MealKinds.All,
                    cookingTimeMinutes: 10, dietLevel: 2, cuisineId: 4,
                    ingredientIds: [4, 12], allergenPositions: [])
            ]);
}
