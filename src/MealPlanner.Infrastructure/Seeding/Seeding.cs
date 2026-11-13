using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Infrastructure.Seeding;

// Seed data for populating the database with allergens, ingredients, and dishes
internal static class SeedData
{
    // Main food allergens
    public static readonly (string Code, string Name)[] Allergens =
    [
        ("MILK", "Milk"),
        ("EGG", "Egg"),
        ("GLUTEN", "Gluten"),
        ("NUTS", "Nuts"),
        ("PEANUT", "Peanut"),
        ("FISH", "Fish"),
        ("SHELLFISH", "Shellfish"),
        ("SOY", "Soy"),
        ("SESAME", "Sesame"),
        ("CELERY", "Celery"),
        ("MUSTARD", "Mustard"),
        ("HONEY", "Honey")
    ];

    // Reference ingredient. Price of 100 grams is in euro cents.
    // Calories also per 100 grams. Also stores typical amount
    // of the ingredient used in one serving.
    public sealed record IngredientSeed(
        string Name,
        IngredientCategory Category,
        int PricePer100g,
        int CaloriesPer100g,
        int TypicalGrams,
        string[] AllergenCodes);

    private static IngredientSeed I(
        string name,
        IngredientCategory category,
        int price,
        int calories,
        int typicalGrams,
        params string[] allergens) =>
        new(name, category, price, calories, typicalGrams, allergens);
    public static readonly IngredientSeed[] Ingredients =
    [
        // --- grains and cereals ---
        I("oat flakes", IngredientCategory.Grain, 18, 366, 60, "GLUTEN"),
        I("buckwheat", IngredientCategory.Grain, 18, 343, 80),
        I("rice", IngredientCategory.Grain, 14, 344, 80),
        I("millet", IngredientCategory.Grain, 9, 348, 80),
        I("pearl barley", IngredientCategory.Grain, 7, 320, 80, "GLUTEN"),
        I("pasta", IngredientCategory.Grain, 11, 337, 100, "GLUTEN"),
        I("wheat flour", IngredientCategory.Grain, 6, 364, 50, "GLUTEN"),
        I("bulgur", IngredientCategory.Grain, 22, 342, 80, "GLUTEN"),
        I("quinoa", IngredientCategory.Grain, 65, 368, 70),
        I("rye bread", IngredientCategory.Grain, 8, 250, 60, "GLUTEN"),

        // --- vegetables ---
        I("potatoes", IngredientCategory.Vegetable, 5, 77, 200),
        I("carrots", IngredientCategory.Vegetable, 6, 41, 80),
        I("onion", IngredientCategory.Vegetable, 5, 40, 60),
        I("beetroot", IngredientCategory.Vegetable, 6, 43, 120),
        I("white cabbage", IngredientCategory.Vegetable, 4, 25, 150),
        I("tomatoes", IngredientCategory.Vegetable, 22, 18, 120),
        I("cucumbers", IngredientCategory.Vegetable, 19, 15, 120),
        I("zucchini", IngredientCategory.Vegetable, 11, 24, 150),
        I("pumpkin", IngredientCategory.Vegetable, 9, 26, 200),
        I("eggplant", IngredientCategory.Vegetable, 18, 24, 150),
        I("bell pepper", IngredientCategory.Vegetable, 24, 27, 100),
        I("mushrooms", IngredientCategory.Vegetable, 26, 22, 100),
        I("spinach", IngredientCategory.Vegetable, 32, 23, 80),
        I("sorrel", IngredientCategory.Vegetable, 28, 22, 80),
        I("broccoli", IngredientCategory.Vegetable, 34, 34, 150),
        I("cauliflower", IngredientCategory.Vegetable, 29, 30, 150),
        I("celery", IngredientCategory.Vegetable, 21, 16, 50, "CELERY"),
        I("green peas", IngredientCategory.Legume, 16, 81, 80),

        // --- legumes ---
        I("lentils", IngredientCategory.Legume, 20, 295, 80),
        I("chickpeas", IngredientCategory.Legume, 24, 364, 80),
        I("beans", IngredientCategory.Legume, 17, 298, 80),
        I("peas", IngredientCategory.Legume, 8, 298, 80),
        I("tofu", IngredientCategory.Legume, 45, 76, 120, "SOY"),

        // --- dairy ---
        I("milk", IngredientCategory.Dairy, 9, 60, 150, "MILK"),
        I("cottage cheese", IngredientCategory.Dairy, 28, 121, 150, "MILK"),
        I("sour cream", IngredientCategory.Dairy, 24, 206, 30, "MILK"),
        I("hard cheese", IngredientCategory.Dairy, 75, 364, 40, "MILK"),
        I("feta", IngredientCategory.Dairy, 68, 264, 50, "MILK"),
        I("plain yogurt", IngredientCategory.Dairy, 21, 66, 150, "MILK"),
        I("butter", IngredientCategory.Dairy, 95, 748, 15, "MILK"),

        // --- eggs ---
        I("chicken egg", IngredientCategory.Egg, 13, 157, 60, "EGG"),

        // --- meat and fish ---
        I("chicken breast", IngredientCategory.Meat, 45, 113, 150),
        I("beef", IngredientCategory.Meat, 89, 187, 150),
        I("pork", IngredientCategory.Meat, 54, 259, 150),
        I("turkey", IngredientCategory.Meat, 52, 84, 150),
        I("cod", IngredientCategory.Fish, 62, 78, 150, "FISH"),
        I("salmon", IngredientCategory.Fish, 120, 208, 150, "FISH"),
        I("herring", IngredientCategory.Fish, 38, 161, 100, "FISH"),
        I("shrimp", IngredientCategory.Fish, 140, 95, 100, "SHELLFISH"),

        // --- fruits and berries ---
        I("apple", IngredientCategory.Fruit, 13, 47, 100),
        I("banana", IngredientCategory.Fruit, 11, 89, 100),
        I("frozen berries", IngredientCategory.Fruit, 36, 45, 60),
        I("raisins", IngredientCategory.Fruit, 28, 264, 30),
        I("dried apricots", IngredientCategory.Fruit, 42, 215, 30),
        I("lemon", IngredientCategory.Fruit, 18, 29, 20),

        // --- nuts and seeds ---
        I("walnuts", IngredientCategory.Nut, 90, 654, 20, "NUTS"),
        I("pine nuts", IngredientCategory.Nut, 280, 673, 15, "NUTS"),
        I("peanuts", IngredientCategory.Nut, 32, 567, 20, "PEANUT"),
        I("sesame seeds", IngredientCategory.Nut, 48, 573, 10, "SESAME"),
        I("sunflower seeds", IngredientCategory.Nut, 22, 578, 15),

        // --- oils, seasonings, and other ---
        I("vegetable oil", IngredientCategory.Oil, 15, 899, 15),
        I("olive oil", IngredientCategory.Oil, 48, 884, 15),
        I("salt", IngredientCategory.Spice, 1, 0, 3),
        I("black pepper", IngredientCategory.Spice, 50, 251, 2),
        I("paprika", IngredientCategory.Spice, 42, 282, 3),
        I("curry", IngredientCategory.Spice, 55, 325, 5),
        I("dill", IngredientCategory.Spice, 30, 38, 10),
        I("parsley", IngredientCategory.Spice, 30, 49, 10),
        I("basil", IngredientCategory.Spice, 52, 27, 10),
        I("garlic", IngredientCategory.Spice, 26, 149, 8),
        I("mustard", IngredientCategory.Spice, 20, 143, 10, "MUSTARD"),
        I("honey", IngredientCategory.Sweetener, 60, 329, 20, "HONEY"),
        I("sugar", IngredientCategory.Sweetener, 7, 399, 15),
        I("jam", IngredientCategory.Sweetener, 35, 271, 30)
    ];

    // Base for the dish: name suitable meal types, cuisine, cooking time,
    // and required ingredients. The diet category is NOT specified here.
    // It is derived from the ingredients by the generator. Specifying it
    // manually would create a second source of truth. Sooner or later,
    // a dish containing chicken would end up being marked as vegetarian.
    public sealed record DishSeed(
    string Name,
    MealTypes SuitableFor,
    Cuisine Cuisine,
    int CookingTimeMinutes,
    string[] CoreIngredients);

    private static DishSeed D(
        string name,
        MealTypes kinds,
        Cuisine cuisine,
        int minutes,
        params string[] core) =>
        new(name, kinds, cuisine, minutes, core);

    private const MealTypes _breakfast = MealTypes.Breakfast;
    private const MealTypes _lunch = MealTypes.Lunch;
    private const MealTypes _dinner = MealTypes.Dinner;
    private const MealTypes _any = MealTypes.All;

    public static readonly DishSeed[] Dishes =
    [
        // --- breakfasts ---
        D("Oatmeal", _breakfast, Cuisine.Other, 10, "oat flakes", "milk"),
        D("Millet porridge", _breakfast, Cuisine.Russian, 25, "millet", "milk", "butter"),
        D("Cottage cheese casserole", _breakfast, Cuisine.Russian, 40, "cottage cheese", "chicken egg", "sugar"),
        D("Cottage cheese pancakes", _breakfast, Cuisine.Russian, 25, "cottage cheese", "chicken egg", "wheat flour"),
        D("Omelet", _breakfast | _dinner, Cuisine.Other, 12, "chicken egg", "milk"),
        D("Fried eggs", _breakfast, Cuisine.Other, 8, "chicken egg", "vegetable oil"),
        D("Pancakes", _breakfast, Cuisine.Russian, 30, "wheat flour", "milk", "chicken egg"),
        D("Granola with yogurt", _breakfast, Cuisine.American, 5, "oat flakes", "plain yogurt"),
        D("Avocado toast", _breakfast, Cuisine.American, 10, "rye bread", "olive oil"),
        D("Rice porridge", _breakfast, Cuisine.Other, 25, "rice", "milk"),

        // --- soups ---
        D("Borscht", _lunch, Cuisine.Russian, 70, "beetroot", "white cabbage", "potatoes", "carrots"),
        D("Cabbage soup", _lunch, Cuisine.Russian, 60, "white cabbage", "potatoes", "carrots", "onion"),
        D("Pea soup", _lunch, Cuisine.Russian, 75, "peas", "potatoes", "carrots"),
        D("Creamy pumpkin soup", _lunch, Cuisine.Mediterranean, 40, "pumpkin", "carrots", "onion"),
        D("Creamy mushroom soup", _lunch, Cuisine.Mediterranean, 45, "mushrooms", "potatoes", "butter"),
        D("Minestrone", _lunch, Cuisine.Italian, 50, "beans", "tomatoes", "pasta", "carrots"),
        D("Fish soup", _lunch, Cuisine.Russian, 50, "cod", "potatoes", "carrots", "onion"),
        D("Lentil soup", _lunch, Cuisine.MiddleEastern, 45, "lentils", "carrots", "onion"),
        D("Sorrel soup", _lunch, Cuisine.Russian, 40, "sorrel", "potatoes", "chicken egg"),
        D("Broccoli soup", _lunch, Cuisine.Mediterranean, 35, "broccoli", "potatoes", "milk"),

        // --- main courses ---
        D("Buckwheat with mushrooms", _lunch | _dinner, Cuisine.Russian, 35, "buckwheat", "mushrooms", "onion"),
        D("Pilaf", _lunch, Cuisine.MiddleEastern, 60, "rice", "carrots", "onion"),
        D("Chickpea pilaf", _lunch | _dinner, Cuisine.MiddleEastern, 55, "rice", "chickpeas", "carrots"),
        D("Pasta with pesto", _lunch | _dinner, Cuisine.Italian, 25, "pasta", "basil", "pine nuts"),
        D("Risotto", _lunch | _dinner, Cuisine.Italian, 40, "rice", "mushrooms", "hard cheese"),
        D("Vegetable stew", _lunch | _dinner, Cuisine.Other, 40, "zucchini", "eggplant", "bell pepper", "tomatoes"),
        D("Mashed potatoes", _dinner, Cuisine.Russian, 30, "potatoes", "milk", "butter"),
        D("Stewed cabbage", _dinner, Cuisine.Russian, 45, "white cabbage", "carrots", "tomatoes"),
        D("Lentil curry", _lunch | _dinner, Cuisine.Asian, 40, "lentils", "curry", "onion"),
        D("Bulgur with vegetables", _lunch | _dinner, Cuisine.MiddleEastern, 30, "bulgur", "bell pepper", "tomatoes"),
        D("Quinoa with vegetables", _lunch | _dinner, Cuisine.American, 25, "quinoa", "broccoli", "olive oil"),
        D("Baked cod", _dinner, Cuisine.Mediterranean, 35, "cod", "lemon", "olive oil"),
        D("Steamed salmon", _dinner, Cuisine.Asian, 25, "salmon", "lemon"),
        D("Chicken with vegetables", _lunch | _dinner, Cuisine.Other, 45, "chicken breast", "bell pepper", "carrots"),
        D("Turkey with buckwheat", _lunch | _dinner, Cuisine.Russian, 50, "turkey", "buckwheat"),
        D("Beef stew", _lunch, Cuisine.Russian, 90, "beef", "carrots", "onion"),
        D("Asian-style tofu", _lunch | _dinner, Cuisine.Asian, 25, "tofu", "bell pepper", "garlic"),
        D("Beans in tomato sauce", _lunch | _dinner, Cuisine.Mediterranean, 40, "beans", "tomatoes", "garlic"),

        // --- salads and light meals ---
        D("Vinaigrette salad", _any, Cuisine.Russian, 30, "beetroot", "potatoes", "carrots", "green peas"),
        D("Feta salad", _any, Cuisine.Mediterranean, 12, "feta", "tomatoes", "cucumbers", "olive oil"),
        D("Cucumber salad", _any, Cuisine.Russian, 8, "cucumbers", "dill", "sour cream"),
        D("Cabbage salad", _any, Cuisine.Russian, 10, "white cabbage", "carrots", "vegetable oil"),
        D("Greek salad", _any, Cuisine.Mediterranean, 15, "feta", "cucumbers", "bell pepper", "olive oil"),
        D("Tuna salad", _any, Cuisine.Mediterranean, 12, "herring", "cucumbers", "chicken egg"),
        D("Spinach with garlic", _dinner, Cuisine.Mediterranean, 12, "spinach", "garlic", "olive oil"),
        D("Baked cauliflower", _dinner, Cuisine.Other, 35, "cauliflower", "olive oil", "paprika")
    ];

    // Addition to the base dishes. Creates dish variants. For example,
    // "Oatmeal" plus "with berries" becomes "Oatmeal with berries",
    // with the same ingredients plus one additional ingredient.
    public sealed record DishVariant(string Suffix, string IngredientName);

    public static readonly DishVariant[] Variants =
    [
        new("with berries", "frozen berries"),
        new("with honey", "honey"),
        new("with nuts", "walnuts"),
        new("with raisins", "raisins"),
        new("with dried apricots", "dried apricots"),
        new("with banana", "banana"),
        new("with apple", "apple"),
        new("with herbs", "parsley"),
        new("with garlic", "garlic"),
        new("with sour cream", "sour cream"),
        new("with cheese", "hard cheese"),
        new("with sesame", "sesame seeds"),
        new("country-style", "dill"),
        new("spicy", "paprika"),
        new("with mushrooms", "mushrooms")
    ];
}
