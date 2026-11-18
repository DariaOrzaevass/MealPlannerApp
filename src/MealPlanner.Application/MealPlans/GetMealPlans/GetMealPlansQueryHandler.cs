using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Application.MealPlans.GetMealPlans;

// Returns the list of meal plans. The handler is simple, because no addition logic is
// required. There is only one query to the database that already returns required response.
internal sealed class GetMealPlansQueryHandler(IMealPlanReadRepository mealPlans)
    : IQueryHandler<GetMealPlansQuery, IReadOnlyCollection<MealPlanSummaryResponse>>
{
    public async Task<Result<IReadOnlyCollection<MealPlanSummaryResponse>>> Handle(
        GetMealPlansQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MealPlanSummaryResponse> summaries =
            await mealPlans.GetSummariesAsync(query.UserId, cancellationToken);

        // Empty list is not an error. New user might have no plans yet
        return Result.Success(summaries);
    }
}
