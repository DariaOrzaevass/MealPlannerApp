using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Allergens;

// Repository interface for allergen
public interface IAllergenRepository
{
    // For retrieving an allergen asynchronously by its unique identifier
    Task<Allergen?> GetByIdAsync(AllergenId id, CancellationToken cancellationToken = default);

    // For getting all allergens asynchronously
    Task<IReadOnlyCollection<Allergen>> GetAllAsync(CancellationToken cancellationToken = default);

    // Max bit position for getting the new one during the creation
    Task<int> GetMaxBitPositionAsync(CancellationToken cancellationToken = default);

    // For adding a new allergen
    void Add(Allergen allergen);
}
