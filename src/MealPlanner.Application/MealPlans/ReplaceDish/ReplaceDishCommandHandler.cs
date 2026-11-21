using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Catalog;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Planning.Models;
using Microsoft.Extensions.Options;

namespace MealPlanner.Application.MealPlans.ReplaceDish;

// Finds the closest feasible replacement and performs the replacement.
// The planning problem is reconstructed from the saved meal plan.
internal sealed class ReplaceDishCommandHandler(
    ICatalogSnapshotProvider snapshots,
    IMealPlanRepository mealPlans)
    : ICommandHandler<ReplaceDishCommand, ReplaceDishResponse>
{
    public async Task<Result<ReplaceDishResponse>> Handle(
    ReplaceDishCommand command,
    CancellationToken cancellationToken)
    {
        var planId = MealPlanId.From(command.MealPlanId);

        MealPlan? plan = await mealPlans.GetByIdAsync(planId, cancellationToken);

        if (plan is null || !plan.IsOwnedBy(command.UserId))
        {
            return Result.Failure<ReplaceDishResponse>(MealPlanErrors.NotFound(planId));
        }

        MealPlanEntry? entry = plan.FindEntry(command.DayNumber, command.MealType);

        if (entry is null)
        {
            return Result.Failure<ReplaceDishResponse>(MealPlanErrors.EntryNotFound);
        }

        if (entry.IsLocked)
        {
            return Result.Failure<ReplaceDishResponse>(MealPlanErrors.EntryLocked);
        }

        CatalogSnapshot snapshot = await snapshots.GetAsync(cancellationToken);

        Result<PlanningSpec> spec = PlanningSpecFactory.Create(plan.Constraints, snapshot);

        if (spec.IsFailure)
        {
            return Result.Failure<ReplaceDishResponse>(spec.Error);
        }

        // Delete later only for compiling
        return Result.Failure<ReplaceDishResponse>(spec.Error);

    }
}
