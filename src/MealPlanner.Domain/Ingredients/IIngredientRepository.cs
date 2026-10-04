using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Ingredients;

// Repository interface for Ingredient
public interface IIngredientRepository
{
    // For retrieving an ingredient asynchronously by its unique identifier
    Task<Ingredient?> GetByIdAsync(IngredientId id, CancellationToken cancellationToken = default);

    // For getting all ingredients asynchronously
    Task<IReadOnlyCollection<Ingredient>> GetAllAsync(CancellationToken cancellationToken = default);

    // For adding a new ingredient
    void Add(Ingredient ingredient);
}

