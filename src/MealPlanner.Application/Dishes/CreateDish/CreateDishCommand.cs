using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Application.Dishes.CreateDish;

// Adds the dish to the catalog. The dish is created unpublished,
// until it is published it does not participate in the plan generation.
// It is impossible to create a dish without ingredients.
public sealed record CreateDishCommand(
    string Name,
    string Description,
    string Recipe,
    decimal Cost,
    string CurrencyCode,
    int Calories,
    MealTypes SuitableFor,
    int CookingTimeMinutes,
    DietType Diet,
    Cuisine Cuisine,
    IReadOnlyList<DishIngredientInput> Ingredients) : ICommand<Guid>;

public sealed record DishIngredientInput(
    Guid IngredientId,
    decimal Quantity,
    MeasurementUnit Unit);
