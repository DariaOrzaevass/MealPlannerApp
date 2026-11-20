using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Planning.Models;

namespace MealPlanner.Application.Catalog.GetCatalogSummary;

// Makes a summary of the catalog snapshot.
// Doesn't access database, because everything it needs is already in the snapshot.
internal sealed class GetCatalogSummaryQueryHandler(ICatalogSnapshotProvider snapshots)
    : IQueryHandler<GetCatalogSummaryQuery, CatalogSummaryResponse>
{
    private const int _mealsPerDay = 3;

    public async Task<Result<CatalogSummaryResponse>> Handle(
        GetCatalogSummaryQuery query,
        CancellationToken cancellationToken)
    {
        CatalogSnapshot snapshot = await snapshots.GetAsync(cancellationToken);
        DishCatalog catalog = snapshot.Catalog;

        if (catalog.Count == 0)
        {
            return Result.Failure<CatalogSummaryResponse>(CatalogErrors.Empty);
        }

        int[] costs = catalog.CostMinorUnits;

        // Lower bound for a meaningful daily budget for the day from 3 cheapest dishes in
        // the catalog. To be more precise dishes also need to be suitable for corresponding
        // meal types, but even this implementation gives the user approximation of the required budget.
        int minDayBudget = costs.Order().Take(_mealsPerDay).Sum();

        var response = new CatalogSummaryResponse(
            catalog.Count,
            catalog.IngredientCount,
            ToMoney(costs.Min()),
            ToMoney(costs.Max()),
            ToMoney((int)Math.Round(costs.Average())),
            ToMoney(minDayBudget),
            snapshot.Currency.Code);

        return response;
    }

    private static decimal ToMoney(int minorUnits) => minorUnits / 100m;
}
