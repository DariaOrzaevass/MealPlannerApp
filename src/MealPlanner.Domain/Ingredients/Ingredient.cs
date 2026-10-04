using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Allergens;

namespace MealPlanner.Domain.Ingredients;

// An ingredient entity representing a specific ingredient
public sealed class Ingredient : Entity<IngredientId>
{
    // The list of allergens of the ingredient
    private readonly List<IngredientAllergen> _allergens = [];

    // Private constructor so that the ingredient can only be created using the Create method with validation
    private Ingredient(IngredientId id, string name, IngredientCategory category)
    : base(id)
    {
        Name = name;
        Category = category;
    }

    // Default constructor for EF core to get allergen from the database
    private Ingredient()
    {
    }

    // The name of the ingredient like "Milk" to display to the user
    public string Name { get; set; } = null!;
    
    // The product category of the ingredient like "Dairy"
    public IngredientCategory Category { get; private set; }

    // The read-only collection of allergens of the ingredient
    public IReadOnlyCollection<IngredientAllergen> Allergens => _allergens.AsReadOnly();

    // Creates a new ingredient only after checking the input
    public static Result<Ingredient> Create(string name, IngredientCategory category)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Ingredient>(IngredientErrors.EmptyName);
        }

        return new Ingredient(IngredientId.New(), name.Trim(), category);
    }

    // Adds an allergen to the ingredient if it does not already have it
    public Result AddAllergen(AllergenId allergenId)
    {
        if (_allergens.Exists(a => a.AllergenId == allergenId))
        {
            return Result.Failure(IngredientErrors.DuplicateAllergen);
        }

        _allergens.Add(new IngredientAllergen(Id, allergenId));

        return Result.Success();
    }

    // Removes an allergen from the ingredient if it has such an allergen
    public Result RemoveAllergen(AllergenId allergenId)
    {
        int removed = _allergens.RemoveAll(a => a.AllergenId == allergenId);

        return removed == 0
            ? Result.Failure(Error.NotFound(
                "Ingredient.AllergenNotFound",
                $"Allergen with ID {allergenId} not found in the ingredient."))
            : Result.Success();
    }

    // Renames the ingredient if the new name is not empty or whitespace
    public Result Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(IngredientErrors.EmptyName);
        }

        Name = name.Trim();

        return Result.Success();
    }
}
