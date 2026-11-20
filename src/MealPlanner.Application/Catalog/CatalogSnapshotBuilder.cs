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

// Converts domain entities to the snapshot for the planning.
// The one of only two places where Planning and Domain meet.
// Everything else works either with Domain or Planning.
public static class CatalogSnapshotBuilder
{
    public static CatalogSnapshot Build(
        IReadOnlyCollection<Dish> dishes,
        IReadOnlyCollection<Ingredient> ingredients,
        IReadOnlyCollection<Allergen> allergens)
    {
        ArgumentNullException.ThrowIfNull(dishes);
        ArgumentNullException.ThrowIfNull(ingredients);
        ArgumentNullException.ThrowIfNull(allergens);

        // Allergen -> bit position in the mask.
        // Mapping is defined in Domain, here it is only transfered
        Dictionary<AllergenId, int> bitByAllergen = allergens
            .ToDictionary(a => a.Id, a => a.BitPosition);

        // Ingredient -> dense index and its allergens. Dense indices are needed
        // for inverted index, which is addressed through array, not dictionary
        var indexByIngredient = new Dictionary<IngredientId, int>(ingredients.Count);
        var allergenBitsByIngredient = new Dictionary<IngredientId, int[]>(ingredients.Count);
        int ingredientIndex = 0;

        foreach (Ingredient ingredient in ingredients)
        {
            indexByIngredient[ingredient.Id] = ingredientIndex++;

            allergenBitsByIngredient[ingredient.Id] =
            [
                .. ingredient.Allergens
                    .Where(a => bitByAllergen.ContainsKey(a.AllergenId))
                    .Select(a => bitByAllergen[a.AllergenId])
            ];
        }

        var descriptors = new List<DishDescriptor>(dishes.Count);
        var dishIds = new List<DishId>(dishes.Count);
        var dishNames = new List<string>(dishes.Count);
        var dishCosts = new List<int>(dishes.Count);
        var indexByDishId = new Dictionary<DishId, int>(dishes.Count);

        Currency currency = dishes.Count == 0
            ? Currency.Eur
            : dishes.First().Cost.Currency;

        foreach (Dish dish in dishes)
        {
            int[] composition =
            [
                .. dish.Ingredients
                    .Where(i => indexByIngredient.ContainsKey(i.IngredientId))
                    .Select(i => indexByIngredient[i.IngredientId])
            ];

            // The allergen set of a dish is derived from its ingredients and is not stored separately.
            int[] allergenPositions =
            [
                .. dish.Ingredients
                    .Where(i => allergenBitsByIngredient.ContainsKey(i.IngredientId))
                    .SelectMany(i => allergenBitsByIngredient[i.IngredientId])
                    .Distinct()
            ];

            // Major currency units are turned into minor for Planning
            var costMinorUnits = (int)Math.Round(dish.Cost.Amount * 100m);

            descriptors.Add(new DishDescriptor(
                costMinorUnits,
                dish.Calories,
                ToMealKinds(dish.SuitableFor),
                dish.CookingTimeMinutes,
                (int)dish.Diet,
                (int)dish.Cuisine,
                composition,
                allergenPositions));

            indexByDishId[dish.Id] = dishIds.Count;
            dishIds.Add(dish.Id);
            dishNames.Add(dish.Name);
            dishCosts.Add(costMinorUnits);
        }

        DishCatalog catalog = DishCatalogBuilder.Build(descriptors);

        // Index, similarity metric, and similarity graph depend
        // only on catalog, so they all are built here and reused
        // by all requests. They are expensive to construct.
        DishIndex index = DishIndex.Build(catalog);
        var similarity = new SimilarityMetric(catalog);

        return new CatalogSnapshot(
            catalog,
            index,
            similarity,
            SimilarityGraph.Build(catalog, index, similarity),
            [.. dishIds],
            [.. dishNames],
            [.. dishCosts],
            indexByDishId,
            indexByIngredient,
            bitByAllergen,
            currency);
    }

    // Converts meal-type flags from the domain enum inti the planning enum.
    // The numeric values currently match, but directly casting should not
    // be done because the first different between enums could silently cause
    // incorrect dish filtering. Explicit conversion will catch these inconsistencies.
    private static MealKinds ToMealKinds(MealTypes types)
    {
        var kinds = MealKinds.None;

        if (types.HasFlag(MealTypes.Breakfast))
        {
            kinds |= MealKinds.Breakfast;
        }

        if (types.HasFlag(MealTypes.Lunch))
        {
            kinds |= MealKinds.Lunch;
        }

        if (types.HasFlag(MealTypes.Dinner))
        {
            kinds |= MealKinds.Dinner;
        }

        if (types.HasFlag(MealTypes.Snack))
        {
            kinds |= MealKinds.Snack;
        }

        return kinds;
    }

    // Converts a Domain meal type into Planning one
    public static MealKinds ToMealKind(MealType type)
    {
#pragma warning disable CS8524
        return type switch
        {
            MealType.Breakfast => MealKinds.Breakfast,
            MealType.Lunch => MealKinds.Lunch,
            MealType.Dinner => MealKinds.Dinner,
            MealType.Snack => MealKinds.Snack
        };
#pragma warning restore CS8524
    }

    // Converts a Planning meal type into Domain one
    public static MealType ToMealType(MealKinds kind) => kind switch
    {
        MealKinds.Breakfast => MealType.Breakfast,
        MealKinds.Lunch => MealType.Lunch,
        MealKinds.Dinner => MealType.Dinner,
        MealKinds.Snack => MealType.Snack,
        _ => throw new ArgumentOutOfRangeException(
            nameof(kind),
            kind,
            "A slot must correspond to exactly one meal type.")
    };
}
