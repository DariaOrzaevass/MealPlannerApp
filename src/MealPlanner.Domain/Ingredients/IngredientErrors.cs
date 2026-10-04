using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Ingredients;

// Class containing all errors related to ingredients
public static class IngredientErrors
{
    // Returns an error if the ingredient with this ID is not found
    public static Error NotFound(IngredientId id) =>
        Error.NotFound("Ingredient.NotFound", $"Ingredient with ID {id} not found.");

    // Returns an error if the ingredient name is empty
    public static readonly Error EmptyName =
        Error.Validation("Ingredient.EmptyName", "Ingredient name cannot be empty.");

    // Returns error if the ingredient already has this allergen
    public static readonly Error DuplicateAllergen =
        Error.Conflict("Ingredient.DuplicateAllergen", "This ingredient already has this allergen");
}
