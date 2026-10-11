using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.DataStructures;

// Inverted ingredient index. Each ingredient has recorded dishes that contain it
public sealed class DishIndex
{
    private readonly int[] _offsets;
    private readonly int[] _dishes;

    // Private constructor so Dish Index can only be made in Build
    private DishIndex(int[] offsets, int[] dishes, int ingredientCount)
    {
        _offsets = offsets;
        _dishes = dishes;
        IngredientCount = ingredientCount;
    }

    // The count of different ingredients
    public int IngredientCount { get; }

    // Builds a reverse index from ingredients to dishes
    public static DishIndex Build(DishCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        int ingredientCount = catalog.IngredientCount;

        // ingredientCount + 1 for offsets later
        var counts = new int[ingredientCount + 1];

        // To not to count one ingredient twice in the dish lastDish stores
        // the last dish to which this ingredient has been procressed
        var lastDish = new int[ingredientCount];
        Array.Fill(lastDish, -1);

        // First loop counts how many dishes contain each ingredient
        for (int dish = 0; dish < catalog.Count; dish++)
        {
            foreach (int ingredient in catalog.IngredientsOf(dish))
            {
                // Checks for duplicates
                if (lastDish[ingredient] == dish)
                {
                    continue;
                }

                lastDish[ingredient] = dish;
                // 0 is reserved for offset
                counts[ingredient + 1]++;
            }
        }

        // Turns counts into prefix sums
        for (int i = 0; i < ingredientCount; i++)
        {
            counts[i + 1] += counts[i];
        }

        // Copy prefix sums into the array of offsets
        var offsets = new int[ingredientCount + 1];
        Array.Copy(counts, offsets, counts.Length);

        // Stores all dishes id
        var dishes = new int[offsets[ingredientCount]];
        var write = new int[ingredientCount];
        Array.Copy(offsets, write, ingredientCount);

        // Recets the duplicate tracking lastDish
        Array.Fill(lastDish, -1);

        // Second loop - for filling the dishes array. 
        for (int dish = 0; dish < catalog.Count; dish++)
        {
            foreach (int ingredient in catalog.IngredientsOf(dish))
            {
                if (lastDish[ingredient] == dish)
                {
                    continue;
                }

                lastDish[ingredient] = dish;
                // The next dish for that ingredient will be stored in the next index
                dishes[write[ingredient]++] = dish;
            }
        }

        return new DishIndex(offsets, dishes, ingredientCount);
    }

    // Returns dish id that contain his ingredient in ascending order
    public ReadOnlySpan<int> DishesWith(int ingredientId)
    {
        // The user mey dislike an ingredient which is not in the catalog
        if (ingredientId < 0 || ingredientId >= IngredientCount)
        {
            return [];
        }

        // Starting position of this ingredient's dishes
        int start = _offsets[ingredientId];

        // End is _offsets[ingredientId + 1] - beginning of another ingredient's dishes
        return _dishes.AsSpan(start, _offsets[ingredientId + 1] - start);
    }

    // How many dishes have this ingredient
    public int DishCountWith(int ingredientId) => DishesWith(ingredientId).Length;

    // Marks all the dishes that have at least one forbidden ingredient
    public int MarkDishesWithAny(ReadOnlySpan<int> ingredientIds, Span<bool> forbidden)
    {
        // How many dishes has been marked forbidden
        int marked = 0;

        foreach (int ingredient in ingredientIds)
        {
            foreach (int dish in DishesWith(ingredient))
            {
                if (forbidden[dish])
                {
                    continue;
                }

                forbidden[dish] = true;
                marked++;
            }
        }

        // Returns the number of forbidden dishes
        return marked;
    }
}
