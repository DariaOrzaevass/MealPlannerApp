using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;

namespace MealPlanner.Application.Catalog.GetCatalogSummary;

// Provides catalog summary that is used for the meal plan generation
public sealed record GetCatalogSummaryQuery : IQuery<CatalogSummaryResponse>;
