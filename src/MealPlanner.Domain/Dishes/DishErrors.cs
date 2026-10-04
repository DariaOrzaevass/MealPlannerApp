using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Dishes;


// Class containing all errors related to dishes
public static class DishErrors
{
    // Returns an error if the dish with this ID is not found
    public static Error NotFound(DishId id) =>
        Error.NotFound("Dish.NotFound", $"Dish with ID {id} not found.");

    // Returns an error if the name of the dish is empty
    public static readonly Error EmptyName =
        Error.Validation("Dish.EmptyName", "Dish name cannot be empty.");


    // Returns an error if the cost of the dish is negative
    public static readonly Error NegativeCost =
        Error.Validation("Dish.NegativeCost", "The cost of the dish cannot be empty.");

    // Returns an error if calorie of the dish are negative
    public static readonly Error NonPositiveCalories =
        Error.Validation("Dish.NonPositiveCalories", "The calorie of the dish must be positive.");

    // Returns an error if the dish does not have a meal type
    public static readonly Error NoMealType =
        Error.Validation("Dish.NoMealType", "A dish must have at least one meal type.");

    // Returns an error if the cooking time is negative
    public static readonly Error NonPositiveCookingTime =
        Error.Validation("Dish.NonPositiveCookingTime", "The cooking time must be positive.");

    // Returns an error if the recipe is empty
    public static readonly Error EmptyRecipe =
        Error.Validation("Dish.EmptyRecipe", "Recipe cannot be empty.");

    // Returns an error if the dish already has this ingredient
    public static readonly Error DuplicateIngredient =
        Error.Conflict("Dish.DuplicateIngredient", "This ingredient is already part of the dish.");

    // Returns an error if the dish does not include this ingredient
    public static readonly Error IngredientNotFound =
        Error.NotFound("Dish.IngredientNotFound", "This ingredient is not included in the dish.");

    // Returns an error if the unknown currency code has been entered
    public static readonly Error UnknownCurrency =
        Error.Validation("Dish.UnknownCurrency", "An unknown currency code has been entered.");

    // Returns an error if the unknown ingredient has been entered
    public static readonly Error UnknownIngredient =
        Error.Problem("Dish.UnknownIngredient", "The non-existing ingredient has been entered.");

    // Returns an error if the dish does not have ingredients
    public static readonly Error NoIngredients =
       Error.Problem("Dish.NoIngredients", "Cannot publish a dish without ingredients.");

    // Returns an error if the amount of the ingredient is negative
    public static readonly Error NonPositiveQuantity =
       Error.Validation("Dish.NonPositiveQuantity", "The amount of the ingredient must be positive.");
}
