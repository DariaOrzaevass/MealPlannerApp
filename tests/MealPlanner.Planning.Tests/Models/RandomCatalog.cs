using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Tests.Models;

// Generates Random Testing Catalog
internal static class RandomCatalog
{
    // Builds a DishCatalog with ingredientIds from 0 to 39
    public static DishCatalog Build(int dishCount, int seed, int ingredientPool = 40)
    {
        // Determinisic random generator
        var random = new Random(seed);

        // List of Dish Descriptors
        var descriptors = new List<DishDescriptor>(dishCount);

        // Generates all dishes
        for (int i = 0; i < dishCount; i++)
        {
            // Each dish fits at least one meal kind, can fit several
            // Random from 1 to 15 - all possible combinations of 4 flags
            var suitableFor = (MealKinds)random.Next(1, 16);

            // Generates random number of ingredients (2 - 6) with random Id
            // Does not cares about actual ingredients just their Id (no duplicates)
            int[] ingredients = [.. Enumerable
                .Range(0, random.Next(2, 7))
                .Select(_ => random.Next(0, ingredientPool))
                .Distinct()];

            // Generates random number of allergens (0 - 2) with random Id (0 - 8)
            // Does not cares about actual allergens just their Id (no duplicates)
            int[] allergens = [.. Enumerable
                .Range(0, random.Next(0, 3))
                .Select(_ => random.Next(0, 9))
                .Distinct()];

            // Adds other random properties + suitableFor, ingredients, allergens
            // Generates new dish and adds it to the descriptors
            descriptors.Add(new DishDescriptor(
                CostMinorUnits: random.Next(3_000, 40_000),
                Calories: random.Next(150, 850),
                SuitableFor: suitableFor,
                CookingTimeMinutes: random.Next(5, 120),
                DietLevel: random.Next(0, 3),
                CuisineId: random.Next(0, 7),
                IngredientIds: ingredients,
                AllergenPositions: allergens));
        }

        // Returns the built Dish Catalog
        return DishCatalogBuilder.Build(descriptors);
    }
}
