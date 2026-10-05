using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Dishes;

// Repository interface for Dish
public interface IDishRepository
{
    // For retrieving a dish asynchronously by its unique identifier
    Task<Dish?> GetByIdAsync(DishId id, CancellationToken cancellationToken = default);

    // For getting all dishes asynchronously
    Task<IReadOnlyCollection<Dish>> GetByIdsAsync(
        IReadOnlyCollection<DishId> ids,
        CancellationToken cancellationToken = default);

    // For all published meals with their ingredients. Needed for the creation of a meal plan
    Task<IReadOnlyCollection<Dish>> GetPublishedWithIngredientsAsync(
        CancellationToken cancellationToken = default);

    // For adding a new dish
    void Add(Dish dish);

    // For removing a dish
    void Remove(Dish dish);
}
