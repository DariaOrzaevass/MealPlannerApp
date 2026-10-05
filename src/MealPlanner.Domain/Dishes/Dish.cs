using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Dishes.Events;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Domain.Dishes;

// A dish entity representing a specific dish
public sealed class Dish : Entity<DishId>
{
    // The list of ingredients of the dish
    private readonly List<DishIngredient> _ingredients = [];

    // Private constructor so that the dish can only be created using the Create method with validation
    private Dish(
        DishId id,
        string name,
        string description,
        string recipe,
        Money cost,
        int calories,
        MealTypes suitableFor,
        int cookingTimeMinutes,
        DietType diet,
        Cuisine cuisine)
        : base(id)
    {
        Name = name;
        Description = description;
        Recipe = recipe;
        Cost = cost;
        Calories = calories;
        SuitableFor = suitableFor;
        CookingTimeMinutes = cookingTimeMinutes;
        Diet = diet;
        Cuisine = cuisine;
        IsPublished = false;
    }

    // Default constructor for EF core to get the dish from the database
    private Dish()
    {
    }

    // The name of the dish to display to the user
    public string Name { get; set; } = null!;

    // Short description of the dish
    public string Description { get; set; } = null!;

    // Recipe of the dish
    public string Recipe { get; set; } = null!;

    // The cost of one portion of the dish
    public Money Cost { get; set; } = null!;

    // The amount of calories in the dish
    public int Calories { get; set; }

    // When we can it this dish (Breakfast, Lunch, etc.)
    public MealTypes SuitableFor { get; set; }

    // How long does it takes to cook this dish
    public int CookingTimeMinutes { get; set; }

    // Is this dish Vegetarian/Vegan or fits any diet
    public DietType Diet { get; set; }

    // Which cuisine is this dish from (Italian for example)
    public Cuisine Cuisine { get; set; }

    // Is this dish published or not
    public bool IsPublished { get; set; }

    // The read-only collection of ingredients of the dish
    public IReadOnlyCollection<DishIngredient> Ingredients => _ingredients.AsReadOnly();

    // Creates a new dish and raises a domain event after checking the input
    public static Result<Dish> Create(
        string name,
        string description,
        string recipe,
        Money cost,
        int calories,
        MealTypes suitableFor,
        int cookingTimeMinutes,
        DietType diet,
        Cuisine cuisine)
    {
        Result validation = Validate(name, recipe, cost, calories, suitableFor, cookingTimeMinutes);

        if (validation.IsFailure)
        {
            return Result.Failure<Dish>(validation.Error);
        }

        var dish = new Dish(
            DishId.New(),
            name.Trim(),
            description?.Trim() ?? string.Empty,
            recipe.Trim(),
            cost,
            calories,
            suitableFor,
            cookingTimeMinutes,
            diet,
            cuisine);

        dish.RaiseDomainEvent(new DishCreatedDomainEvent(dish.Id));

        return dish;
    }

    // Validates the input
    private static Result Validate(
        string name,
        string recipe,
        Money cost,
        int calories,
        MealTypes suitableFor,
        int cookingTimeMinutes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(DishErrors.EmptyName);
        }

        if (string.IsNullOrWhiteSpace(recipe))
        {
            return Result.Failure(DishErrors.EmptyRecipe);
        }

        if (cost.Amount < 0)
        {
            return Result.Failure(DishErrors.NegativeCost);
        }

        if (calories <= 0)
        {
            return Result.Failure(DishErrors.NonPositiveCalories);
        }

        if (suitableFor == MealTypes.None)
        {
            return Result.Failure(DishErrors.NoMealType);
        }

        if (cookingTimeMinutes <= 0)
        {
            return Result.Failure(DishErrors.NonPositiveCookingTime);
        }

        return Result.Success();
    }

    // Adds ingredient to the dish after checking the input
    public Result AddIngredient(IngredientId ingredientId, decimal quantity, MeasurementUnit unit)
    {
        if (quantity <= 0 && unit != MeasurementUnit.ToTaste)
        {
            return Result.Failure(DishErrors.NonPositiveQuantity);
        }

        if (_ingredients.Exists(i => i.IngredientId == ingredientId))
        {
            return Result.Failure(DishErrors.DuplicateIngredient);
        }

        _ingredients.Add(new DishIngredient(Id, ingredientId, quantity, unit));

        return Result.Success();
    }

    // Removes ingredient from the dish and checks whether it succeed
    public Result RemoveIngredient(IngredientId ingredientId)
    {
        int removed = _ingredients.RemoveAll(i => i.IngredientId == ingredientId);

        return removed == 0
            ? Result.Failure(DishErrors.IngredientNotFound)
            : Result.Success();
    }

    // Changes the cost of the dish if the new one is not negative
    public Result ChangeCost(Money cost)
    {
        if (cost.Amount < 0)
        {
            return Result.Failure(DishErrors.NegativeCost);
        }

        Cost = cost;

        return Result.Success();
    }

    // Publishes the dish after making sure that it has ingredients and raises the domain event
    public Result Publish()
    {
        if (_ingredients.Count == 0)
        {
            return Result.Failure(DishErrors.NoIngredients);
        }

        if (IsPublished)
        {
            return Result.Success();
        }

        IsPublished = true;
        RaiseDomainEvent(new DishPublishedDomainEvent(Id));

        return Result.Success();
    }

    // Unpublishes the dish and raises the domain event
    public void Unpublish()
    {
        if (!IsPublished)
        {
            return;
        }

        IsPublished = false;
        RaiseDomainEvent(new DishUnpublishedDomainEvent(Id));
    }
}
