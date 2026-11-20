using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shared;
using MealPlanner.Planning.DataStructures;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Scoring;
using MealPlanner.Planning.Solvers;

namespace MealPlanner.Application.Catalog;

// Catalog snapshot that contains everything needed for meal planning +
// tables for converting values back to domain identifiers. Planners work with
// dish indices from 0 to n-1, not domain indentifiers. This allows dictionaries
// to be replaced with arrays with sequential memory access. But we also need
// to convert indices back to domain identifiers, so snapshot also has
// conversion tables with catalog itself to make sure they are consistent.
public sealed class CatalogSnapshot
{
    internal CatalogSnapshot(
        DishCatalog catalog,
        DishIndex index,
        SimilarityMetric similarity,
        SimilarityGraph similarityGraph,
        DishId[] dishIds,
        string[] dishNames,
        int[] dishCostMinorUnits,
        Dictionary<DishId, int> indexByDishId,
        Dictionary<IngredientId, int> indexByIngredientId,
        Dictionary<AllergenId, int> bitByAllergenId,
        Currency currency)
    {
        Catalog = catalog;
        Index = index;
        Similarity = similarity;
        SimilarityGraph = similarityGraph;
        DishIds = dishIds;
        DishNames = dishNames;
        DishCostMinorUnits = dishCostMinorUnits;
        IndexByDishId = indexByDishId;
        IndexByIngredientId = indexByIngredientId;
        BitByAllergenId = bitByAllergenId;
        Currency = currency;
    }

    // Catalog in the form that is used by the solvers
    public DishCatalog Catalog { get; }

    // Inverted index "ingredient -> dishes"
    public DishIndex Index { get; }

    // Similarity Metric. Is build together with catalog because
    // cache table depend only on catalog and needs O(n^2) work
    public SimilarityMetric Similarity { get; }

    // Dish similarity graph. Build together with snapshot for same reason
    // as Similarity Metric (depend only on catalog and needs O(n^2) work)
    public SimilarityGraph SimilarityGraph { get; }

    // Dish index in the planning -> domain identifier
    public IReadOnlyList<DishId> DishIds { get; }

    // Dish index -> dish name. Needed for the UI
    public IReadOnlyList<string> DishNames { get; }

    // Dish index -> cost in Minor Units 
    public IReadOnlyList<int> DishCostMinorUnits { get; }

    // Domain identifier -> dish index in the planning
    public IReadOnlyDictionary<DishId, int> IndexByDishId { get; }

    // ingredient identifier -> its index in planning.
    // Used to convert user's disliked ingredients into indices
    public IReadOnlyDictionary<IngredientId, int> IndexByIngredientId { get; }

    // Allergen identifier -> bit position in the mask.
    // Used for building user's allergen mask.
    public IReadOnlyDictionary<AllergenId, int> BitByAllergenId { get; }

    // Catalog currency. Planning is working with integer minor
    // units, so the currency is stored once for the snapshot.
    public Currency Currency { get; }

    // Number of dishes
    public int DishCount => Catalog.Count;

    // Returns the cost of a dish as a monetary value for storing in a meal plan.
    public Money CostOf(int dishIndex) =>
        new(DishCostMinorUnits[dishIndex] / 100m, Currency);
}
