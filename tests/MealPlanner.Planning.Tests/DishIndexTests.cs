using MealPlanner.Planning.DataStructures;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Scoring;

namespace MealPlanner.Planning.Tests;

// Tests for DishIndex
public sealed class DishIndexTests
{
    // Tests DishIndex against a simple scan
    [Fact]
    public void DishesWith_matches_a_direct_scan()
    {
        // Creates a small catalog
        DishCatalog catalog = CatalogSamples.Create(
            new CatalogSamples.Draft(10, 100, MealKinds.Breakfast, [1, 3]),
            new CatalogSamples.Draft(20, 200, MealKinds.Lunch, [1, 2]),
            new CatalogSamples.Draft(30, 300, MealKinds.Breakfast | MealKinds.Dinner, [3]),
            new CatalogSamples.Draft(40, 400, MealKinds.Snack, []));

        // Builds reverse index (what dishes contain a specific ingredient)
        DishIndex index = DishIndex.Build(catalog);

        // Loop through all ingredients
        for (int ingredient = 0; ingredient < catalog.IngredientCount; ingredient++)
        {
            // A reference list with dishes that contain this ingredient
            var expected = new List<int>();

            // For every dish checks if it contains this ingredient and adds if it does
            for (int dish = 0; dish < catalog.Count; dish++)
            {
                if (catalog.IngredientsOf(dish).Contains(ingredient))
                {
                    expected.Add(dish);
                }
            }

            // Checks that the reference is equal to the result of the
            // DishIndex.DishesWith turned into array for comparison
            Assert.Equal(expected, index.DishesWith(ingredient).ToArray());
        }

        // Checks that invalid id (negative or too big) return null
        Assert.Empty(index.DishesWith(-1).ToArray());
        Assert.Empty(index.DishesWith(catalog.IngredientCount).ToArray());
    }

    // Checks the Jaccard implementation
    [Fact]
    public void Jaccard_merges_sorted_lists()
    {
        Assert.Equal(1.0 / 3.0, SimilarityMetric.Jaccard([1, 2], [2, 3]), precision: 9);
        Assert.Equal(1, SimilarityMetric.Jaccard([1, 4], [1, 4]));
        Assert.Equal(0, SimilarityMetric.Jaccard([1], [2]));
        Assert.Equal(1, SimilarityMetric.Jaccard([], []));
    }

    // Checks the SimilarityMetric works correctly
    [Fact]
    public void Identical_dishes_stay_at_similarity_one()
    {
        // Creates small catalog with identical dishes
        DishCatalog catalog = CatalogSamples.Create(
            new CatalogSamples.Draft(10, 200, MealKinds.Breakfast, [1, 4]),
            new CatalogSamples.Draft(10, 200, MealKinds.Breakfast, [1, 4]));

        // Checks that Between returns 1 for identical dishes
        Assert.Equal(1, new SimilarityMetric(catalog).Between(0, 1));
    }

    // Checks that it is impossible to create a dish with duplicate ingredients
    [Fact]
    public void Create_rejects_duplicate_ingredient_ids()
    {
        // Tries to create one dish that has 2 same ingredients and expects an exception to be thrown
        ArgumentException error = Assert.Throws<ArgumentException>(() => CreateOne([0, 2], [1, 1], 3));

        // Checks that exception says that invalid parameter is ingredientIds
        Assert.Equal("ingredientIds", error.ParamName);
    }

    // Checks that it is impossible to create a dish with unsorted ingredients
    [Fact]
    public void Create_rejects_unsorted_ingredient_ids()
    {
        // Tries to create one dish that has unsorted ingredients and expects an exception to be thrown
        ArgumentException error = Assert.Throws<ArgumentException>(() => CreateOne([0, 2], [2, 1], 3));

        // Checks that exception says that invalid parameter is ingredientIds
        Assert.Equal("ingredientIds", error.ParamName);
    }

    // Checks that it is impossible to create a dish with offsets not starting with 0
    [Fact]
    public void Create_rejects_offsets_that_do_not_start_at_zero()
    {
        // Tries to create one dish that has offsets not starting with 0 (starts from 1 here)
        ArgumentException error = Assert.Throws<ArgumentException>(() => CreateOne([1, 2], [1], 3));

        // Checks that exception says that invalid parameter is ingredientOffsets
        Assert.Equal("ingredientOffsets", error.ParamName);
    }

    // Creates a catalog with one dish
    private static DishCatalog CreateOne(int[] offsets, int[] ingredientIds, int ingredientCount) =>
        DishCatalog.Create(
            costMinorUnits: [10],
            calories: [100],
            suitableFor: [MealKinds.Breakfast],
            allergens: [AllergenMask.Empty],
            cookingTimeMinutes: [10],
            dietLevels: [0],
            cuisineIds: [0],
            ingredientOffsets: offsets,
            ingredientIds: ingredientIds,
            ingredientCount: ingredientCount);
}
