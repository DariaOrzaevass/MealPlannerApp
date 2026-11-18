using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Dishes.CreateDish;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.Dishes;

// Adds a dish to the catalog
internal sealed class CreateDishEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("dishes", async (
            CreateDishCommand command,
            ICommandHandler<CreateDishCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/api/dishes/{result.Value}", new { dishId = result.Value })
                : ApiResults.Problem(result);
        })
        .RequireAuthorization(Policies.Administrator)
        .WithName("CreateDish")
        .WithSummary("Add a dish")
        .WithDescription(
            "Creates a dish with its ingredients. The dish is created as unpublished " +
            "and does not participate in meal plan generation until it is published.")
        .WithTags(Tags.Dishes)
        .Produces<object>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden);
}
