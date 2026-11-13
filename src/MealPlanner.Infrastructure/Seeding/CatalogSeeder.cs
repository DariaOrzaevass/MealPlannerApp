using System;
using System.Collections.Generic;
using System.Text;
using Bogus;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shared;
using MealPlanner.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Infrastructure.Seeding;

// Populates the catalog with testing data
public sealed class CatalogSeeder(ApplicationDbContext context, ILogger<CatalogSeeder> logger)
{
    // Random seed used for generation, fixed for reproducibility.
    private const int _seed = 20260925;

    // Number of dish variants to create in addition to base dishes.
    private const int _variantCount = 210;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Set<Allergen>().AnyAsync(cancellationToken))
        {
            logger.LogInformation("The catalogue is already populated. Skipping seeding.");

            return;
        }

        logger.LogInformation("Populating the catalogue...");

        Dictionary<string, Allergen> allergens = SeedAllergens();
        Dictionary<string, (Ingredient Entity, SeedData.IngredientSeed Seed)> ingredients =
            SeedIngredients(allergens);

        int dishCount = SeedDishes(ingredients);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Catalogue populated: {Allergens} allergens, {Ingredients} ingredients, {Dishes} dishes.",
            allergens.Count,
            ingredients.Count,
            dishCount);
    }

    // Bit position is the allergen's index in the list, bitmask relies on it.
    private Dictionary<string, Allergen> SeedAllergens()
    {
        var created = new Dictionary<string, Allergen>(StringComparer.Ordinal);

        for (int position = 0; position < SeedData.Allergens.Length; position++)
        {
            (string code, string name) = SeedData.Allergens[position];

            Allergen allergen = Allergen.Create(name, code, position).Value;

            context.Add(allergen);
            created[code] = allergen;
        }

        return created;
    }

    private Dictionary<string, (Ingredient, SeedData.IngredientSeed)> SeedIngredients(
        Dictionary<string, Allergen> allergens)
    {
        var created = new Dictionary<string, (Ingredient, SeedData.IngredientSeed)>(
            StringComparer.Ordinal);

        foreach (SeedData.IngredientSeed seed in SeedData.Ingredients)
        {
            Ingredient ingredient = Ingredient.Create(seed.Name, seed.Category).Value;

            foreach (string code in seed.AllergenCodes)
            {
                ingredient.AddAllergen(allergens[code].Id);
            }

            context.Add(ingredient);
            created[seed.Name] = (ingredient, seed);
        }

        return created;
    }

    private int SeedDishes(
        Dictionary<string, (Ingredient Entity, SeedData.IngredientSeed Seed)> ingredients)
    {
        var faker = new Faker { Random = new Randomizer(_seed) };
        var names = new HashSet<string>(StringComparer.Ordinal);
        int count = 0;

        // Firstly base dishes are created - they are the core of the catalogue.
        foreach (SeedData.DishSeed seed in SeedData.Dishes)
        {
            if (TryCreateDish(faker, ingredients, seed, variant: null, names) is not null)
            {
                count++;
            }
        }

        // Then variants are made. The base dish is selected randomly,
        // so some base dishes have more variants, like in a real catalog.
        for (int i = 0; i < _variantCount; i++)
        {
            SeedData.DishSeed seed = faker.PickRandom(SeedData.Dishes);
            SeedData.DishVariant variant = faker.PickRandom(SeedData.Variants);

            if (TryCreateDish(faker, ingredients, seed, variant, names) is not null)
            {
                count++;
            }
        }

        return count;
    }

    // Creates a dish. Returns null if the dish with this name already exists, because
    // repeated combinations of a variant dishes are unavoidable during random selection.
    private Dish? TryCreateDish(
       Faker faker,
       Dictionary<string, (Ingredient Entity, SeedData.IngredientSeed Seed)> ingredients,
       SeedData.DishSeed seed,
       SeedData.DishVariant? variant,
       HashSet<string> usedNames)
    {
        string name = variant is null ? seed.Name : $"{seed.Name} {variant.Suffix}";

        if (!usedNames.Add(name))
        {
            return null;
        }

        var composition = new List<(string Name, decimal Quantity, MeasurementUnit Unit)>();

        foreach (string ingredientName in seed.CoreIngredients)
        {
            (decimal quantity, MeasurementUnit unit) =
                PickQuantity(faker, ingredients[ingredientName].Seed);

            composition.Add((ingredientName, quantity, unit));
        }

        if (variant is not null && !seed.CoreIngredients.Contains(variant.IngredientName))
        {
            (decimal quantity, MeasurementUnit unit) =
                PickQuantity(faker, ingredients[variant.IngredientName].Seed);

            composition.Add((variant.IngredientName, quantity, unit));
        }

        // Salt is used in most dishes, so it is added as a background ingredient without affecting price or calories.
        if (faker.Random.Bool(0.8f) && !composition.Exists(c => c.Name == "salt"))
        {
            composition.Add(("salt", 0m, MeasurementUnit.ToTaste));
        }

        // Dish cost is accumulated in euro cents.
        int costMinorUnits = 0;
        int calories = 0;

        foreach ((string ingredientName, decimal quantity, _) in composition)
        {
            SeedData.IngredientSeed ingredientSeed = ingredients[ingredientName].Seed;
            decimal hundredGrams = quantity / 100m;

            costMinorUnits += (int)(ingredientSeed.PricePer100g * hundredGrams);
            calories += (int)(ingredientSeed.CaloriesPer100g * hundredGrams);
        }

        DietType diet = DeriveDiet(composition.Select(c => ingredients[c.Name].Seed));

        Dish dish = Dish.Create(
            name,
            BuildDescription(seed, variant),
            BuildRecipe(seed, composition),
            new Money(Math.Max(1, costMinorUnits) / 100m, Currency.Eur),
            Math.Max(50, calories),
            seed.SuitableFor,
            seed.CookingTimeMinutes,
            diet,
            seed.Cuisine).Value;

        foreach ((string ingredientName, decimal quantity, MeasurementUnit unit) in composition)
        {
            dish.AddIngredient(ingredients[ingredientName].Entity.Id, quantity, unit);
        }

        dish.Publish();
        context.Add(dish);

        return dish;
    }

    // Determines the quantity of the ingredient -> its typical
    // serving size, varied by approximately one third.
    // This prevents dishes from having same prices and calories.
    private static (decimal Quantity, MeasurementUnit Unit) PickQuantity(
        Faker faker,
        SeedData.IngredientSeed seed)
    {
        double factor = faker.Random.Double(0.7, 1.3);
        var grams = (int)Math.Round(seed.TypicalGrams * factor);

        return (Math.Max(1, grams), MeasurementUnit.Gram);
    }

    // The diet type is derived from ingredients, not specified manually.
    // Otherwise accidently dish with chicken could incorrectly be marked vegetarian.
    private static DietType DeriveDiet(IEnumerable<SeedData.IngredientSeed> composition)
    {
        var strictest = DietType.Vegan;

        foreach (SeedData.IngredientSeed seed in composition)
        {
            DietType required = seed.Category switch
            {
                IngredientCategory.Meat or IngredientCategory.Fish => DietType.Omnivore,
                IngredientCategory.Dairy or IngredientCategory.Egg => DietType.Vegetarian,
                // Honey is produced by bees and therefore is not suitable for a vegan diet.
                _ when seed.AllergenCodes.Contains("HONEY") => DietType.Vegetarian,
                _ => DietType.Vegan
            };

            if (required < strictest)
            {
                strictest = required;
            }
        }

        return strictest;
    }
    private static string BuildDescription(SeedData.DishSeed seed, SeedData.DishVariant? variant)
    {
        string basis = $"{seed.Name} - a {CuisineName(seed.Cuisine)} cuisine dish";

        return variant is null
            ? $"{basis}. Approximate cooking time: {seed.CookingTimeMinutes} minutes."
            : $"{basis}, variant: {variant.Suffix}. "
                + $"Approximate cooking time: {seed.CookingTimeMinutes} minutes.";
    }

    private static string BuildRecipe(
        SeedData.DishSeed seed,
        List<(string Name, decimal Quantity, MeasurementUnit Unit)> composition)
    {
        IEnumerable<string> parts = composition
            .Where(c => c.Unit != MeasurementUnit.ToTaste)
            .Select(c => $"{c.Name} - {c.Quantity:0} {UnitName(c.Unit)}");

        return $"Prepare the ingredients: {string.Join(", ", parts)}. "
            + $"Cook for approximately {seed.CookingTimeMinutes} minutes and add salt to taste.";
    }

    private static string UnitName(MeasurementUnit unit) => unit switch
    {
        MeasurementUnit.Gram => "g",
        MeasurementUnit.Milliliter => "ml",
        MeasurementUnit.Piece => "pc",
        MeasurementUnit.Tablespoon => "tbsp",
        MeasurementUnit.Teaspoon => "tsp",
        _ => "to taste"
    };

    private static string CuisineName(Cuisine cuisine) => cuisine switch
    {
        Cuisine.Russian => "Russian",
        Cuisine.Italian => "Italian",
        Cuisine.Asian => "Asian",
        Cuisine.Mediterranean => "Mediterranean",
        Cuisine.American => "American",
        Cuisine.MiddleEastern => "Middle Eastern",
        _ => "European"
    };
}
