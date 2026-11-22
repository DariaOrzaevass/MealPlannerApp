using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.MealPlans.GetMealPlan;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Users;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.MealPlans;

// Returns one complete meal plan with details about each day
internal sealed class GetMealPlanEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("meal-plans/{id:guid}", async (
            Guid id,
            IUserContext userContext,
            IQueryHandler<GetMealPlanQuery, MealPlanResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<UserId> userId = await userContext.GetUserIdAsync(cancellationToken);

            if (userId.IsFailure)
            {
                return ApiResults.Problem(userId);
            }

            Result<MealPlanResponse> result =
                await handler.Handle(new GetMealPlanQuery(id, userId.Value), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result);
        })
        .WithName("GetMealPlan")
        .WithSummary("Get one meal plan")
        .WithDescription(
            "Returns a meal plan with details about each day including dishes," +
            "calories, and the cost of each day. If a plan belongs to another user," +
            "404 is returned similarly to the situation with nonexistent plan.")
        .WithTags(Tags.MealPlans)
        .Produces<MealPlanResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);
}
