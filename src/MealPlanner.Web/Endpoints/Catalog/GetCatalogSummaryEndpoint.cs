using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Catalog.GetCatalogSummary;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.Catalog;

// Returns catalog summary (number of dishes, ingredients + price ranges)
internal sealed class GetCatalogSummaryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("catalog/summary", async (
            IQueryHandler<GetCatalogSummaryQuery, CatalogSummaryResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<CatalogSummaryResponse> result =
                await handler.Handle(new GetCatalogSummaryQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : ApiResults.Problem(result);
        })
        // Available to everyone. It is the first thing a user can see
        // about the application, so authentication is not required.
        .AllowAnonymous()
        .WithName("GetCatalogSummary")
        .WithSummary("Catalog summary")
        .WithDescription(
            "Shows how many dishes and ingredients are available and price range of dishes." +
            "It helps the user in choosing a reasonable busget for the plan generation.")
        .WithTags(Tags.Catalog)
        .Produces<CatalogSummaryResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);
}
