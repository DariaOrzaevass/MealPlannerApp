using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Catalog.GetCatalogOptions;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.Catalog;

// Returns reference data for ingredients and alergens.
// FR2 - the user should select allergens and disliked ingredients
// to exclude, therefore the client needs a way to obtain their
// indentifiers (API clients need a dedicated endpoint).
internal sealed class GetCatalogOptionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("catalog/options", async (
            IQueryHandler<GetCatalogOptionsQuery, CatalogOptionsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<CatalogOptionsResponse> result =
                await handler.Handle(new GetCatalogOptionsQuery(), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result);
        })
        .WithName("GetCatalogOptions")
        .WithSummary("Reference data for constraints")
        .WithDescription(
            "Returns allergens and ingredients with their identifiers." +
            "They are needed to construct the request for" +
            "meal-planning generation, where allergens and" +
            "disliked ingredients are specified by identifiers.")
        .WithTags(Tags.Catalog)
        .Produces<CatalogOptionsResponse>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);
}
