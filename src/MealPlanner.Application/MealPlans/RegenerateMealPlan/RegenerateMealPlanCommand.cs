using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.MealPlans.GenerateMealPlan;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Users;

namespace MealPlanner.Application.MealPlans.RegenerateMealPlan;

// Regenerates a plan to get a different result with same constraints.
// Since solvers are deterministic, it is important to change the input problem.
// Dishes from the currrent plan aside from locked ones are excluded from candidate domains.
public sealed record RegenerateMealPlanCommand(
    UserId UserId,
    Guid MealPlanId,
    PlannerAlgorithm? Algorithm = null) : ICommand<GenerateMealPlanResponse>;
