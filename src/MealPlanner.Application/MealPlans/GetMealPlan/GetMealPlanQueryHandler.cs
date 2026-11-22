using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Catalog;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.MealPlans;

namespace MealPlanner.Application.MealPlans.GetMealPlan;

// Builds a meal plan response to be displayed
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
                .Select(group => BuildDay(plan, group, snapshot))
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
        IGrouping<int, MealPlanEntry> day,
        CatalogSnapshot snapshot)
    {
        List<MealPlanEntryResponse> meals =
        [
            .. day
                .OrderBy(entry => entry.MealType)
                .Select(entry => new MealPlanEntryResponse(
                    entry.MealType,
                    entry.DishId.Value,
                    NameOf(entry.DishId, snapshot),
                    CaloriesOf(entry.DishId, snapshot),
                    entry.Cost.Amount,
                    entry.IsLocked))
        ];

        return new MealPlanDayResponse(
            day.Key,
            plan.StartDate.AddDays(day.Key - 1),
            meals.Sum(meal => meal.Calories),
            meals.Sum(meal => meal.Cost),
            meals);
    }

    private static string NameOf(DishId dishId, CatalogSnapshot snapshot) =>
        snapshot.IndexByDishId.TryGetValue(dishId, out int index)
            ? snapshot.DishNames[index]
            : "(dish is no longer published)";

    private static int CaloriesOf(DishId dishId, CatalogSnapshot snapshot) =>
        snapshot.IndexByDishId.TryGetValue(dishId, out int index)
            ? snapshot.Catalog.Calories[index]
            : 0;
}
