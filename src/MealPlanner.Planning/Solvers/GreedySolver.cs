using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Solvers;

// The Greedy Algorithm Solver
public sealed class GreedySolver : IMealPlanSolver
{
    //
    private const int _repairIterationLimit = 256;

    // The name of the algorithm
    public string Name => "Greedy";

    public PlanSolution Solve(PlanningProblem problem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);

        // Records the start to see later the execution time of the solver
        long start = Stopwatch.GetTimestamp();

        if (problem.Domains.HasEmptyDomain)
        {
            return PlanSolution.Infeasible(
                "There is no available dish for one of the meal slots.",
                exploredNodes: 0,
                Stopwatch.GetTimestamp() - start);
        }

        var state = new SearchState(problem);

        // Delete later, only to compile
        return PlanSolution.Infeasible(
            "There is no available dish for one of the meal slots.",
            exploredNodes: 0,
            Stopwatch.GetTimestamp() - start);
    }
}
