using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Application.Catalog;

// Errors related to working with catalog. Separate class and not just strings
// where they are used, so same error code is written in all parts of the application.
public static class CatalogErrors
{
    // The catalog is empty, there are no dishes to generate a plan from.
    public static readonly Error Empty = Error.Problem(
    "Catalog.Empty",
    "the catalog does not contain any published dishes.");
}
