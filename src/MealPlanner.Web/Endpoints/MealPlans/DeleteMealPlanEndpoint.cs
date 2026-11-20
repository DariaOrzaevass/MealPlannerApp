using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.MealPlans.DeleteMealPlan;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Users;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.MealPlans;

// Delets a saved meal plan
internal sealed class DeleteMealPlanEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
    app.MapDelete("meal-plans/{id:guid}", async (
        Guid id,
        IUserContext userContext,
        ICommandHandler<DeleteMealPlanCommand> handler,
        CancellationToken cancellationToken) =>
    {
        Result<UserId> userId = await userContext.GetUserIdAsync(cancellationToken);

        if (userId.IsFailure)
        {
            return ApiResults.Problem(userId);
        }

        Result result = await handler.Handle(
            new DeleteMealPlanCommand(userId.Value, id),
            cancellationToken);

        return result.IsSuccess ? Results.NoContent() : ApiResults.Problem(result);
    })
    .WithName("DeleteMealPlan")
    .WithSummary("Delete meal plan")
    .WithDescription(
        "Delets the meal plan with its entries. A plan that belongs to" +
        "other user and non-existen one both return 404. By that the" +
        "error code cannot be used to find out IDs of existent meal plans.")
    .WithTags(Tags.MealPlans)
    .Produces(StatusCodes.Status204NoContent)
    .ProducesProblem(StatusCodes.Status404NotFound);
}
