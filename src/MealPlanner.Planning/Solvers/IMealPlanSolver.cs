using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Solvers;

// Interface for all algorithm solvers of the application
public interface IMealPlanSolver
{
    // Name of the solver algorithm
    string Name { get; }

    // Creates a meal plan. If no solution can be found because
    // of the users constraints - it is also a result - Infeasible
    PlanSolution Solve(PlanningProblem problem, CancellationToken cancellationToken = default);
}
