using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Dishes.SetDishPublication;
using MealPlanner.Web.Extensions;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Web.Endpoints.Dishes;

// Publishes or unpublishes a dish
internal sealed class SetDishPublicationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("dishes/{id:guid}/publication", async (
            Guid id,
            PublicationRequest request,
            ICommandHandler<SetDishPublicationCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(
                new SetDishPublicationCommand(id, request.Published),
                cancellationToken);

            return result.IsSuccess ? Results.NoContent() : ApiResults.Problem(result);
        })
        .RequireAuthorization(Policies.Administrator)
        .WithName("SetDishPublication")
        .WithSummary("Dish publication")
        .WithDescription(
            "Enables or disables dish partivipation in the plan generation. " +
            "Any change invalidates the snapshot of the catalog.")
        .WithTags(Tags.Dishes)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

    // Should or not the dish participate in the meal plan generation
    internal sealed record PublicationRequest(bool Published);
}
