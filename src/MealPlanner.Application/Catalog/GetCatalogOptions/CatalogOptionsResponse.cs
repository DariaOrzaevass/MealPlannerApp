using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Application.Catalog.GetCatalogOptions;

// Reference data from which the user chooses constraints.
// Single response for both lists, because they are always used together.
public sealed record CatalogOptionsResponse(
    IReadOnlyList<AllergenOption> Allergens,
    IReadOnlyList<IngredientOption> Ingredients);

public sealed record AllergenOption(Guid Id, string Name, string Code);

public sealed record IngredientOption(Guid Id, string Name, IngredientCategory Category);
