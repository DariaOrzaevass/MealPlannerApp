using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.MealPlans.GetMealPlans;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Users;
using MealPlanner.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Repositories;

// Projects meal plans in list items using one query. Applying Select before materialization
// is the main reason of this class existance. Expression is translated in a list of selected
// database columns and only ten required fields are retrieved instead of the full entity with entries.
internal sealed class MealPlanReadRepository(ApplicationDbContext context)
    : IMealPlanReadRepository
{
    public async Task<IReadOnlyCollection<MealPlanSummaryResponse>> GetSummariesAsync(
    UserId userId,
    CancellationToken cancellationToken = default) =>
    await context.Set<MealPlan>()
        .Where(p => p.UserId == userId)
        .OrderByDescending(p => p.CreatedOnUtc)
        .Select(p => new MealPlanSummaryResponse(
            p.Id.Value,
            p.Name,
            p.StartDate,
            p.Constraints.DurationDays,
            p.TotalCost.Amount,
            p.Constraints.Budget.Amount,
            p.TotalCost.Currency.Code,
            p.Algorithm,
            p.GenerationMs,
            p.CreatedOnUtc))
        .ToListAsync(cancellationToken);
}
