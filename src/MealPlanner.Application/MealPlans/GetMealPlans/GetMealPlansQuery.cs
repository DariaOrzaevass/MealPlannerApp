using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Users;

namespace MealPlanner.Application.MealPlans.GetMealPlans;

// The list of the user's meal plans. First are the new ones
public sealed record GetMealPlansQuery(UserId UserId)
    : IQuery<IReadOnlyCollection<MealPlanSummaryResponse>>;
