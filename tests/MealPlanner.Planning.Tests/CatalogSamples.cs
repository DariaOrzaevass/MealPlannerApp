using MealPlanner.Planning.DataStructures;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Tests;

// Helps to easier create small DishCatalog
internal static class CatalogSamples
{
    // Small temporary description of a testing dish
    internal readonly record struct Draft(
        int Cost,
        int Calories,
        MealKinds SuitableFor,
        int[] Ingredients,
        AllergenMask Allergens = default,
        int CookingMinutes = 20,
        int DietLevel = 0,
        int CuisineId = 0);

    // Takes Draft objects and create an actual DishCatalog out of them
    // params for easier creation of Draft[]
    public static DishCatalog Create(params Draft[] dishes)
    {
        // Checks that array of darts is not null
        ArgumentNullException.ThrowIfNull(dishes);

        // Creates arrays for all properties
        int count = dishes.Length;
        var costs = new int[count];
        var calories = new int[count];
        var suitableFor = new MealKinds[count];
        var allergens = new AllergenMask[count];
        var cooking = new int[count];
        var diets = new int[count];
        var cuisines = new int[count];

        // Storage of ingredients
        var offsets = new int[count + 1];
        var ingredientIds = new List<int>();
        int ingredientCount = 0;

        for (int dish = 0; dish < count; dish++)
        {
            // Gets the current dish and checks that it has Ingredients
            Draft draft = dishes[dish];
            ArgumentNullException.ThrowIfNull(draft.Ingredients);

            // Copies dish properties from draft into corresponding array
            costs[dish] = draft.Cost;
            calories[dish] = draft.Calories;
            suitableFor[dish] = draft.SuitableFor;
            allergens[dish] = draft.Allergens;
            cooking[dish] = draft.CookingMinutes;
            diets[dish] = draft.DietLevel;
            cuisines[dish] = draft.CuisineId;

            // Where dish ingredients starts
            offsets[dish] = ingredientIds.Count;

            foreach (int ingredientId in draft.Ingredients)
            {
                // Adds ingredient
                ingredientIds.Add(ingredientId);

                // Calculates ingredientCount
                if (ingredientId + 1 > ingredientCount)
                {
                    ingredientCount = ingredientId + 1;
                }
            }
        }

        // Final offset stores the end of the last dish
        offsets[count] = ingredientIds.Count;

        // Passes all arrays into real validated DishCatalog.Create()
        return DishCatalog.Create(
            costs,
            calories,
            suitableFor,
            allergens,
            cooking,
            diets,
            cuisines,
            offsets,
            [.. ingredientIds],
            ingredientCount);
    }
}
