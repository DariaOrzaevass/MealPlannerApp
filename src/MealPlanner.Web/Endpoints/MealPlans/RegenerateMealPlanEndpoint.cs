using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.MealPlans.GenerateMealPlan;
using MealPlanner.Application.MealPlans.RegenerateMealPlan;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Users;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.MealPlans;

// Regenerates the meal plan to create a different version
internal sealed class RegenerateMealPlanEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("meal-plans/{id:guid}/regenerate", async (
            Guid id,
            RegenerateRequest? request,
            IUserContext userContext,
            ICommandHandler<RegenerateMealPlanCommand, GenerateMealPlanResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<UserId> userId = await userContext.GetUserIdAsync(cancellationToken);

            if (userId.IsFailure)
            {
                return ApiResults.Problem(userId);
            }

            Result<GenerateMealPlanResponse> result = await handler.Handle(
                new RegenerateMealPlanCommand(userId.Value, id, request?.Algorithm),
                cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result);
        })
        .WithName("RegenerateMealPlan")
        .WithSummary("Regenerate meal plan")
        .WithDescription(
            "Creates a different version of the meal plan with the same constraints." +
            "Locked entries are not changed, but dishes from other entries are declared as" +
            "rejected and excluded from the new version. The solver can stay the same as in" +
            "the first version if no solver in specified, but it is also possible to change it.")
        .WithTags(Tags.MealPlans)
        .Produces<GenerateMealPlanResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

    internal sealed record RegenerateRequest(PlannerAlgorithm? Algorithm);
}
