using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Application.Catalog.GetCatalogSummary;

// Response that contains a summary of the catalog, a separate response
// type instead of returning domain entity to only show data required.
// Client do not receive unnecessary recipes, identifiers, or
// properties that can accidentaly expose additional data from database.
public sealed record CatalogSummaryResponse(
    int DishCount,
    int IngredientCount,
    decimal MinDishCost,
    decimal MaxDishCost,
    decimal AverageDishCost,
    decimal MinDayBudget,
    string Currency);
