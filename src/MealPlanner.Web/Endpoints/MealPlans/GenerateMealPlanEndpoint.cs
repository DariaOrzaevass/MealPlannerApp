using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.MealPlans;
using MealPlanner.Application.MealPlans.GenerateMealPlan;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Users;
using MealPlanner.Web.Contracts;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.MealPlans;

// Generate a meal plan
internal sealed class GenerateMealPlanEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
    app.MapPost("meal-plans", async (
        GenerateMealPlanRequest request,
        IUserContext userContext,
        ICommandHandler<GenerateMealPlanCommand, GenerateMealPlanResponse> handler,
        CancellationToken cancellationToken) =>
    {

        Result<UserId> userId = await userContext.GetUserIdAsync(cancellationToken);

        if (userId.IsFailure)
        {
            return ApiResults.Problem(userId);
        }

        Result<GenerateMealPlanResponse> result =
            await handler.Handle(ToCommand(request, userId.Value), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : ApiResults.Problem(result);
    })
    .WithName("GenerateMealPlan")
    .WithSummary("Generate meal plan")
    .WithDescription(
        "Generates a meal plan with a chosen solver and saves it. Returns " +
        "search metrics: execution time, number of explored nodes, proportion " +
        "of budget used, diversity index. There are different types of failures. " +
        "Planning.NoCandidates means that no dishes satisfy the constraints. " +
        "Planning.Infeasible means that it has been established that no feasible " +
        "plan exist for this problem. Planning.Exhausted means that the " +
        "solver did not finish whithin the allowed search limits.")
    .WithTags(Tags.MealPlans)
    .Produces<GenerateMealPlanResponse>()
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status400BadRequest);

    private static GenerateMealPlanCommand ToCommand(
        GenerateMealPlanRequest request,
        UserId userId) =>
        new(
            userId,
            request.Name,
            request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            new PlanningConstraintsInput(
                request.Budget,
                request.CurrencyCode ?? Currency.Eur.Code,
                request.DurationDays,
                request.MealsPerDay ?? [MealType.Breakfast, MealType.Lunch, MealType.Dinner],
                request.Diet,
                request.ExcludedAllergens ?? [],
                request.DislikedIngredients ?? [],
                request.MaxRepeatsPerPlan,
                request.MinDaysBetweenRepeats,
                request.MinDailyCalories,
                request.MaxDailyCalories,
                request.MaxCookingTimeMinutes,
                request.PreferredCuisines ?? []),
            request.Algorithm);
}
