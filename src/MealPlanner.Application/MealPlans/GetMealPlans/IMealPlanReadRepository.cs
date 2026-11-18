using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Users;

namespace MealPlanner.Application.MealPlans.GetMealPlans;

// Reads meal plans in a ready for displaying form, without loading the
// aggregate. For reading data, so retrieves only what needs to be displayed
public interface IMealPlanReadRepository
{
    Task<IReadOnlyCollection<MealPlanSummaryResponse>> GetSummariesAsync(
    UserId userId,
    CancellationToken cancellationToken = default);
}
