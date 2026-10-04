using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Domain.Dishes;

// Represents the relationship between a dish and its ingredients 
public sealed class DishIngredient
{
    // Constructor to create a new DishIngredient relationship
    internal DishIngredient(
    DishId dishId,
    IngredientId ingredientId,
    decimal quantity,
    MeasurementUnit unit)
    {
        DishId = dishId;
        IngredientId = ingredientId;
        Quantity = quantity;
        Unit = unit;
    }

    // Default constructor for EF core
    private DishIngredient()
    {
    }

    // The Id of the specific dish
    public DishId DishId { get; set; } = null!;

    // The Id of the specific ingredient
    public IngredientId IngredientId { get; set; } = null!;

    // The amount of the ingredient
    public decimal Quantity { get; set; }

    // The meausurement units in which amount is specified
    public MeasurementUnit Unit { get; private set; }
}
