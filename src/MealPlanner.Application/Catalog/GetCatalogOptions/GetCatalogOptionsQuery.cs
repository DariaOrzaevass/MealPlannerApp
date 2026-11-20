using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;

namespace MealPlanner.Application.Catalog.GetCatalogOptions;

// Returns allergen and ingredients reference data for the constraints form.
public sealed record GetCatalogOptionsQuery : IQuery<CatalogOptionsResponse>;
