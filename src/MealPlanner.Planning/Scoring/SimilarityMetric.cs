using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Scoring;

// Similarity of two dishes from 0 to 1. Used for the diversity of the meal plan
public sealed class SimilarityMetric
{
#pragma warning disable IDE1006 // Naming Styles
    private const int MaxCachedDishes = 2048;
#pragma warning restore IDE1006 // Naming Styles

    private readonly DishCatalog _catalog;
    private readonly double _ingredientWeight;
    private readonly double _cuisineWeight;
    private readonly double _calorieWeight;

    private readonly float[]? _cache;

    public SimilarityMetric(
        DishCatalog catalog,
        double ingredientWeight = 0.7,
        double cuisineWeight = 0.2,
        double calorieWeight = 0.1)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        _catalog = catalog;
        _ingredientWeight = ingredientWeight;
        _cuisineWeight = cuisineWeight;
        _calorieWeight = calorieWeight;

        if (catalog.Count is > 0 and <= MaxCachedDishes)
        {
            _cache = BuildCache(catalog.Count);
        }
    }

    // Catalog from which the similarity metric is built. Needed for
    // validation to not to use another catalog (will get wrong results)
    internal DishCatalog Catalog => _catalog;

    // Returns similarity between two dishes from 0 to 1
    public double Between(int dishA, int dishB)
    {
        // The same dish gets 1.0
        if (dishA == dishB)
        {
            return 1.0;
        }

        // Computes similiraty if _cash is null othervise retrives data from there
        return _cache is null
            ? Compute(dishA, dishB)
            : _cache[(dishA * _catalog.Count) + dishB];
    }

    // Computes similarity between two dishes
    private double Compute(int dishA, int dishB)
    {
        // Compares dishes by ingredients, cuisine, and by calories
        double byIngredients = Jaccard(_catalog.IngredientsOf(dishA), _catalog.IngredientsOf(dishB));
        double byCuisine = _catalog.CuisineIds[dishA] == _catalog.CuisineIds[dishB] ? 1.0 : 0.0;
        double byCalories = CalorieCloseness(_catalog.Calories[dishA], _catalog.Calories[dishB]);

        // Multiply by weights to change the importance of each similarity
        double total = (_ingredientWeight * byIngredients)
            + (_cuisineWeight * byCuisine)
            + (_calorieWeight * byCalories);

        // Add all weights in normalizer and return total divided by normalizer, if it's not equal to 0
        double normalizer = _ingredientWeight + _cuisineWeight + _calorieWeight;

        return normalizer == 0 ? 0 : total / normalizer;
    }

    // Precomputes similarity between dishes and fills the cashe.
    // It is simmetric so computes similarity of a half, but stores for both dishes
    private float[] BuildCache(int dishCount)
    {
        var cache = new float[dishCount * dishCount];

        for (int a = 0; a < dishCount; a++)
        {
            // Similarity of each dish with itself is 1
            cache[(a * dishCount) + a] = 1f;

            for (int b = a + 1; b < dishCount; b++)
            {
                // Computes simularity
                var value = (float)Compute(a, b);

                // Stores the result of Compute for both dishes
                cache[(a * dishCount) + b] = value;
                cache[(b * dishCount) + a] = value;
            }
        }

        return cache;
    }

    // Reverse of Between. non-negative so can be used for Dijkstra's weighting of nodes
    public double DistanceBetween(int dishA, int dishB) => 1.0 - Between(dishA, dishB);

    // Returns Jaccard Similarity (size of intersection/size of union)
    internal static double Jaccard(ReadOnlySpan<int> first, ReadOnlySpan<int> second)
    {
        if (first.IsEmpty && second.IsEmpty)
        {
            return 1.0;
        }

        if (first.IsEmpty || second.IsEmpty)
        {
            return 0.0;
        }

        // How many values appear in both
        int intersection = 0;

        // Looks for intersections
        foreach (int item in first)
        {
            if (second.Contains(item))
            {
                intersection++;
            }
        }

        // The size of the union
        int union = first.Length + second.Length - intersection;

        // If union is 0 - avoid divizion by 0
        return union == 0 ? 0.0 : (double)intersection / union;
    }

    private static double CalorieCloseness(int first, int second)
    {
        // If different by 400.0 calories or more - completely different
        const double Threshold = 400.0;

        // Absolute difference between calories of first dish and second
        double difference = Math.Abs(first - second);

        // If the difference smaller than Threshold - calculate closeness from 0 to 1
        return difference >= Threshold ? 0.0 : 1.0 - (difference / Threshold);
    }
}
