using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Users;

namespace MealPlanner.Application.MealPlans.GenerateMealPlan;

// Generates and saves a meal plan. constraints are provided through PlanningConstraintsInput.
// The same set of constraints is also used when comparing solvers, and duplicating this 
// accross two commands would rewuire two identical validators. UserId is added by endpoint
// so users will not be able to generate a meal plan on behalf of another user.
public sealed record GenerateMealPlanCommand(
    UserId UserId,
    string Name,
    DateOnly StartDate,
    PlanningConstraintsInput Constraints,
    PlannerAlgorithm Algorithm) : ICommand<GenerateMealPlanResponse>;
