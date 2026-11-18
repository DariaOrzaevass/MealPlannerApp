using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Dishes.GetDishes;
using MealPlanner.Web.Extensions;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Web.Endpoints.Dishes;

// Returns a page of the dish catalog to the administrator
internal sealed class GetDishesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
    app.MapGet("dishes", async (
        string? search,
        bool? publishedOnly,
        int? page,
        int? pageSize,
        IQueryHandler<GetDishesQuery, DishPageResponse> handler,
        CancellationToken cancellationToken) =>
    {
        Result<DishPageResponse> result = await handler.Handle(
            new GetDishesQuery(search, publishedOnly, page ?? 1, pageSize ?? 20),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result);
    })
    .RequireAuthorization(Policies.Administrator)
    .WithName("GetDishes")
    .WithSummary("Dish catalog")
    .WithDescription("Paginated list of dishes with filtering by name and publication status.")
    .WithTags(Tags.Dishes)
    .Produces<DishPageResponse>()
    .ProducesProblem(StatusCodes.Status403Forbidden);
}
