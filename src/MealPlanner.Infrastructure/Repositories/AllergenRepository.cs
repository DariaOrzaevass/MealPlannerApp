using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Allergens;
using MealPlanner.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Repositories;

// Repository for the Allergen entity
internal sealed class AllergenRepository(ApplicationDbContext context)
    : Repository<Allergen, AllergenId>(context), IAllergenRepository
{
    public async Task<IReadOnlyCollection<Allergen>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await Context.Set<Allergen>()
            .OrderBy(a => a.BitPosition)
            .ToListAsync(cancellationToken);

    // Finds the largest occupied bit position used by allergen.
    // It is needed to assign next position when adding a new allergen.
    public async Task<int> GetMaxBitPositionAsync(CancellationToken cancellationToken = default)
    {
        // MaxAsync would fail on an empty table, so nullable type
        // is used here, and -1 if the allergen list is empty.
        int? maximum = await Context.Set<Allergen>()
            .MaxAsync(a => (int?)a.BitPosition, cancellationToken);

        return maximum ?? -1;
    }
}
