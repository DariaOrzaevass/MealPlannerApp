using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Scoring;

namespace MealPlanner.Planning.Tests;

// Objective function without the variety term that is minimized in layered graph by shortest path
internal static class PathObjective
{
    public static double Evaluate(PlanningProblem problem, IReadOnlyList<int> assignment) =>
    ObjectiveFunction.EvaluateWithoutVariety(problem, [.. assignment]);
}
