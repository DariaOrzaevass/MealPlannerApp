using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Ingredients;

// An ingredient entity representing a specific ingredient
public sealed class Ingredient : Entity<IngredientId>
{
    // Private constructor so that the ingredient can only be created using the Create method with validation
    private Ingredient(IngredientId id, string name)
    : base(id)
    {
        Name = name;
    }

    // Default constructor for EF core to get allergen from the database
    private Ingredient()
    {
    }

    // The name of the ingredient like "Milk" to display to the user
    public string Name { get; set; } = null!;


}
