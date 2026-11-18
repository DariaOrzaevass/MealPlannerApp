using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Application.MealPlans.GenerateMealPlan;

// The result of the meal plan generation. Aside from ID search
// metrics are also returned. They are needed for easy algorithm
// evaluation. Interface can simply show how different solvers
// give different execution time, costs, and diversity for same constraints.
public sealed record GenerateMealPlanResponse(
    Guid MealPlanId,
    string Algorithm,
    decimal TotalCost,
    double BudgetUsage,
    double DiversityIndex,
    long GenerationMs,
    long ExploredNodes,
    bool IsProvenOptimal);
