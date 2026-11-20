using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Application.Catalog.GetCatalogOptions;

// Returns reference data for the constraint form
// The data is read through repositories, not from catalog snapshot.
// The snapshot has indices and masks, but the form needs actual names
// to display to the user. The tables are small and change rarely, so
// 2 simple reads are cheaper than storing 2 more lists in the snapshot.
internal sealed class GetCatalogOptionsQueryHandler(
    IAllergenRepository allergens,
    IIngredientRepository ingredients)
    : IQueryHandler<GetCatalogOptionsQuery, CatalogOptionsResponse>
{
    public async Task<Result<CatalogOptionsResponse>> Handle(
        GetCatalogOptionsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Allergen> allAllergens = await allergens.GetAllAsync(cancellationToken);
        IReadOnlyCollection<Ingredient> allIngredients =
            await ingredients.GetAllAsync(cancellationToken);

        var response = new CatalogOptionsResponse(
            [
                .. allAllergens
                    .OrderBy(a => a.Name, StringComparer.CurrentCulture)
                    .Select(a => new AllergenOption(a.Id.Value, a.Name, a.Code))
            ],
            [
                .. allIngredients
                    .OrderBy(i => i.Category)
                    .ThenBy(i => i.Name, StringComparer.CurrentCulture)
                    .Select(i => new IngredientOption(i.Id.Value, i.Name, i.Category))
            ]);

        return response;
    }
}
