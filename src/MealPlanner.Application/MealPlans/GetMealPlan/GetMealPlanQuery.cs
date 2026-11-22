using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Users;

namespace MealPlanner.Application.MealPlans.GetMealPlan;

// Returns the details of a complete plan
public sealed record GetMealPlanQuery(Guid MealPlanId, UserId UserId)
    : IQuery<MealPlanResponse>;
