using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Catalog;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.MealPlans;

namespace MealPlanner.Application.MealPlans.GetMealPlan;

//
internal sealed class GetMealPlanQueryHandler(
    IMealPlanRepository mealPlans,
    ICatalogSnapshotProvider snapshots)
    : IQueryHandler<GetMealPlanQuery, MealPlanResponse>
{
    public async Task<Result<MealPlanResponse>> Handle(
        GetMealPlanQuery query,
        CancellationToken cancellationToken)
    {
        var id = MealPlanId.From(query.MealPlanId);

        MealPlan? plan = await mealPlans.GetByIdAsync(id, cancellationToken);

        if (plan is null || !plan.IsOwnedBy(query.UserId))
        {
            return Result.Failure<MealPlanResponse>(MealPlanErrors.NotFound(id));
        }

        CatalogSnapshot snapshot = await snapshots.GetAsync(cancellationToken);

        return Build(plan, snapshot);
    }

    private static MealPlanResponse Build(MealPlan plan, CatalogSnapshot snapshot)
    {
        List<MealPlanDayResponse> days =
        [
            .. plan.Entries
                .GroupBy(entry => entry.DayNumber)
                .OrderBy(group => group.Key)
                .Select(group => BuildDay(plan, group))
        ];

        return new MealPlanResponse(
            plan.Id.Value,
            plan.Name,
            plan.StartDate,
            plan.Constraints.DurationDays,
            plan.TotalCost.Amount,
            plan.Constraints.Budget.Amount,
            plan.TotalCost.Currency.Code,
            plan.Algorithm,
            plan.GenerationMs,
            plan.ExploredNodes,
            plan.DiversityIndex(),
            days);
    }

    private static MealPlanDayResponse BuildDay(
        MealPlan plan,
        IGrouping<int, MealPlanEntry> day)
    {
        List<MealPlanEntryResponse> meals =
[
    .. day
                .OrderBy(entry => entry.MealType)
                .Select(entry => new MealPlanEntryResponse(
                    entry.MealType,
                    entry.DishId.Value,
                    entry.IsLocked))
];

        return new MealPlanDayResponse(
            day.Key,
            plan.StartDate.AddDays(day.Key - 1),
            meals);
    }
}
