using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Dishes;
using MealPlanner.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Repositories;

// Repository for the Dish entity
internal sealed class DishRepository(ApplicationDbContext context)
    : Repository<Dish, DishId>(context), IDishRepository
{

    // For a collection of dishes
    public async Task<IReadOnlyCollection<Dish>> GetByIdsAsync(
    IReadOnlyCollection<DishId> ids,
    CancellationToken cancellationToken = default)
    {
        // Materializes collection into array. EF Core passes the collection as a parameter and
        // expands it on the server side. For this collection must already be materialized rather
        // than being a deffered enumeration that would have to be evaluated while building the query.
        DishId[] keys = [.. ids];

        return await Context.Set<Dish>()
            .Where(d => keys.Contains(d.Id))
            .ToListAsync(cancellationToken);
    }

    // Returns all published dishes with their ingredients
    public async Task<IReadOnlyCollection<Dish>> GetPublishedWithIngredientsAsync(
           CancellationToken cancellationToken = default) =>
           await Context.Set<Dish>()
               .Where(d => d.IsPublished)
               .Include(d => d.Ingredients)
               .AsNoTracking()
               .OrderBy(d => d.Id)
               .ToListAsync(cancellationToken);
}
