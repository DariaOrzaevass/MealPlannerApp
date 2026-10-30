using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Solvers;

public sealed class BacktrackingSolver : IMealPlanSolver
{
    private readonly BacktrackingHeuristics _heuristics;
    private readonly TimeSpan _timeLimit;
    private readonly long _nodeLimit;
    private readonly bool _optimize;
    private readonly bool _warmStart;

    public BacktrackingSolver(
    BacktrackingHeuristics heuristics = BacktrackingHeuristics.Recommended,
    TimeSpan? timeLimit = null,
    long nodeLimit = 20_000_000,
    bool optimize = false,
    bool warmStart = true)
    {
        _heuristics = heuristics;
        _timeLimit = timeLimit ?? TimeSpan.FromSeconds(2);
        _nodeLimit = nodeLimit;
        _optimize = optimize;
        _warmStart = warmStart;
    }

    public string Name => _optimize
        ? "Branch and Bound"
        : _heuristics == BacktrackingHeuristics.Recommended
            ? "Backtracking"
            : $"Backtracking ({_heuristics})";

    // The main solving method that takes a problem and returns a plan solution
    public PlanSolution Solve(PlanningProblem problem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);

        // Records the start to see later the execution time of the solver
        long start = Stopwatch.GetTimestamp();

        if (problem.Domains.HasEmptyDomain)
        {
            return PlanSolution.Infeasible(
                "There is no valid dish for one of the meal slots.",
                exploredNodes: 0,
                Stopwatch.GetTimestamp() - start);
        }

        // Delete later, for compliling only
        return PlanSolution.Infeasible(
            "There is no valid dish for one of the meal slots.",
            exploredNodes: 0,
            Stopwatch.GetTimestamp() - start);
    }
}
