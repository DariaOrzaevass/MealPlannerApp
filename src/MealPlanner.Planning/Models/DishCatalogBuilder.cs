using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;

namespace MealPlanner.Planning.Models;

// Builds a dish catalog from a list of dish descriptors
public static class DishCatalogBuilder
{
    public static DishCatalog Build(IReadOnlyList<DishDescriptor> descriptors)
    {
        // The list of descriptors cannot be null to build a catalog
        ArgumentNullException.ThrowIfNull(descriptors);

        // Number of dishes
        int count = descriptors.Count;

        // Arrays for dish properties
        var costs = new int[count];
        var calories = new int[count];
        var suitableFor = new MealKinds[count];
        var allergens = new AllergenMask[count];
        var cookingTime = new int[count];
        var dietLevels = new int[count];
        var cuisineIds = new int[count];

        // Offsets for Ingredients
        var offsets = new int[count + 1];
        int maxIngredientId = -1;

        // Loops through all dishes to get the offsets of
        // dish ingredients and largest ingredient id
        for (int dish = 0; dish < count; dish++)
        {
            DishDescriptor descriptor = descriptors[dish];
            offsets[dish + 1] = offsets[dish] + descriptor.IngredientIds.Count;

            foreach (int ingredient in descriptor.IngredientIds)
            {
                // Cannot have negative id for an ingredient
                ArgumentOutOfRangeException.ThrowIfNegative(ingredient);

                if (ingredient > maxIngredientId)
                {
                    maxIngredientId = ingredient;
                }
            }
        }

        // Creates array for ingredientIds
        var ingredientIds = new int[offsets[count]];

        // Loops through dishes again actually building the catalog
        for (int dish = 0; dish < count; dish++)
        {
            DishDescriptor descriptor = descriptors[dish];

            // Validates that cost, Calories, and cooking time are not negative
            ArgumentOutOfRangeException.ThrowIfNegative(descriptor.CostMinorUnits);
            ArgumentOutOfRangeException.ThrowIfNegative(descriptor.Calories);
            ArgumentOutOfRangeException.ThrowIfNegative(descriptor.CookingTimeMinutes);

            // Simply copies properties from dish descriptor
            costs[dish] = descriptor.CostMinorUnits;
            calories[dish] = descriptor.Calories;
            suitableFor[dish] = descriptor.SuitableFor;
            cookingTime[dish] = descriptor.CookingTimeMinutes;
            dietLevels[dish] = descriptor.DietLevel;
            cuisineIds[dish] = descriptor.CuisineId;

            // Empty Allergen Mask
            ulong mask = 0UL;

            // Sets bits for present allergens with bitwise OR
            foreach (int position in descriptor.AllergenPositions)
            {
                mask |= AllergenMask.FromPosition(position).Bits;
            }

            // Creates an Allergen Mask of the dish
            allergens[dish] = new AllergenMask(mask);

            // Writes ingredients to the ingredientIds array
            int write = offsets[dish];

            foreach (int ingredient in descriptor.IngredientIds)
            {
                ingredientIds[write++] = ingredient;
            }
        }

        // Returns new Dish Catalog
        return new DishCatalog(
            costs,
            calories,
            suitableFor,
            allergens,
            cookingTime,
            dietLevels,
            cuisineIds,
            offsets,
            ingredientIds,
            maxIngredientId + 1);
    }
}
