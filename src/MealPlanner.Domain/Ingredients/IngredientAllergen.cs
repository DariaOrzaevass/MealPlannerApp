using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Allergens;

namespace MealPlanner.Domain.Ingredients;

// Represents the relationship between an ingredient and allergens
public sealed class IngredientAllergen
{

    // Constructor to create a new IngredientAllergen relationship
    internal IngredientAllergen(IngredientId ingredientId, AllergenId allergenId)
    {
        IngredientId = ingredientId;
        AllergenId = allergenId;
    }

    // Default constructor for EF core to get allergen from the database
    private IngredientAllergen()
    {
    }

    // The Id of the specific ingredient
    public IngredientId IngredientId { get; private set; } = null!;

    // The Id of the specific allergen
    public AllergenId AllergenId { get; private set; } = null!;
}
