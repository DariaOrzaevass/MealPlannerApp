using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.MealPlans.ReplaceDish;
using MealPlanner.Web.Contracts;
using MealPlanner.Web.Extensions;
using MealPlanner.Domain.Users;
using MealPlanner.Domain.Abstractions;


namespace MealPlanner.Web.Endpoints.MealPlans;

// Replace a dish in a slot of a meal plan
public class ReplaceDishEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
    app.MapPost("meal-plans/{id:guid}/replace", async (
        Guid id,
        ReplaceDishRequest request,
        IUserContext userContext,
        ICommandHandler<ReplaceDishCommand, ReplaceDishResponse> handler,
        CancellationToken cancellationToken) =>
    {
        Result<UserId> userId = await userContext.GetUserIdAsync(cancellationToken);

        if (userId.IsFailure)
        {
            return ApiResults.Problem(userId);
        }

        Result<ReplaceDishResponse> result = await handler.Handle(
            new ReplaceDishCommand(
                userId.Value,
                id,
                request.DayNumber,
                request.MealType,
                request.RejectedDishes ?? []),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result);
    })
    .WithName("ReplaceDish")
    .WithSummary("Replace a Dish")
    .WithDescription(
        "Looks for the closes feasible replacement for the dish" +
        "in the specific slot and puts the replavement in the meal" +
        "plan. The replacement is chosen by the system, the user" +
        "only indicates which dish to change and does not choose the" +
        "replacements themselves. The search is done with Dijkstra's" +
        "algorithm ran in the similarity graph.")
    .WithTags(Tags.MealPlans)
    .Produces<ReplaceDishResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);
}
