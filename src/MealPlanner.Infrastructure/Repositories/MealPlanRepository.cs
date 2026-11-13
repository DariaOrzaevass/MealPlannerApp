using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Users;
using MealPlanner.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Repositories;

// Repository for the MealPlan entity
internal sealed class MealPlanRepository(ApplicationDbContext context)
    : Repository<MealPlan, MealPlanId>(context), IMealPlanRepository
{
    // A meal plan with all its entries. Whithout them the plan
    // is useless. Any operation on the plan (display, replace
    // a dish, recalculate the cost) requires all entries.
    public override async Task<MealPlan?> GetByIdAsync(
        MealPlanId id,
        CancellationToken cancellationToken = default) =>
        await Context.Set<MealPlan>()
            .Include(p => p.Entries)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    // Retrieves all meal plans of a specific user
    public async Task<IReadOnlyCollection<MealPlan>> GetByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default) =>
        await Context.Set<MealPlan>()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedOnUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
