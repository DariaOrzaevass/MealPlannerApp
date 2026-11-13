using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Repositories;

// Repository for the Ingredient entity
internal sealed class IngredientRepository(ApplicationDbContext context)
    : Repository<Ingredient, IngredientId>(context), IIngredientRepository
{
    // Returns all ingredients in the database
    public async Task<IReadOnlyCollection<Ingredient>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await Context.Set<Ingredient>()
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);
}
