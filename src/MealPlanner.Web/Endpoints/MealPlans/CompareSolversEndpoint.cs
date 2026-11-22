using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.MealPlans.CompareSolvers;
using MealPlanner.Application.MealPlans;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Shared;
using MealPlanner.Web.Contracts;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.MealPlans;

// Runs all solvers on same planning problem. POST even though nothing is saved because
// of the size of the input. Planning constraints contains of many fields, some even
// have lists of identifiers. Passing them all through the query string is not convenient
// and GET requests with a body are not supported reliably by many intermediaries.
internal sealed class CompareSolversEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("meal-plans/compare", async (
            GenerateMealPlanRequest request,
            IQueryHandler<CompareSolversQuery, IReadOnlyCollection<SolverComparisonRow>> handler,
            CancellationToken cancellationToken) =>
        {
            var constraints = new PlanningConstraintsInput(
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
                request.PreferredCuisines ?? []);

            Result<IReadOnlyCollection<SolverComparisonRow>> result =
                await handler.Handle(new CompareSolversQuery(constraints), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result);
        })
        .WithName("CompareSolvers")
        .WithSummary("Compare solvers")
        .WithDescription(
            "Runs four solvers using the same planning constraints and returns" +
            "the metrics of solvers. Nothing is saved. Name, StartDate, and" +
            "Algorithm fields in the request body are not used.")
        .WithTags(Tags.MealPlans)
        .Produces<IReadOnlyCollection<SolverComparisonRow>>()
        .ProducesProblem(StatusCodes.Status400BadRequest);
}
