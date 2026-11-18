using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.MealPlans.GetMealPlans;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Users;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.MealPlans;

// Returns saved plans of the curent user
internal sealed class GetMealPlansEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
       app.MapGet("meal-plans", async (
           IUserContext userContext,
           IQueryHandler<GetMealPlansQuery, IReadOnlyCollection<MealPlanSummaryResponse>> handler,
           CancellationToken cancellationToken) =>
       {
           Result<UserId> userId = await userContext.GetUserIdAsync(cancellationToken);

           if (userId.IsFailure)
           {
               return ApiResults.Problem(userId);
           }

           Result<IReadOnlyCollection<MealPlanSummaryResponse>> result =
               await handler.Handle(new GetMealPlansQuery(userId.Value), cancellationToken);

           return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result);
       })
       .WithName("GetMealPlans")
       .WithSummary("Meal plan list")
       .WithDescription("Returns saved plans of the curent user. Newest first. No plan entries.")
       .WithTags(Tags.MealPlans)
       .Produces<IReadOnlyCollection<MealPlanSummaryResponse>>();
}
