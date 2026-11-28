using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.MealPlans;

namespace MealPlanner.Application.MealPlans.CompareSolvers;

// The result of one solver for the same problem
public sealed record SolverComparisonRow(
    PlannerAlgorithm Algorithm,
    string Name,
    bool Solved,
    string? Failure,
    decimal TotalCost,
    double BudgetUsage,
    double DiversityIndex,
    long ElapsedMs,
    long ExploredNodes,
    bool IsProvenOptimal);
