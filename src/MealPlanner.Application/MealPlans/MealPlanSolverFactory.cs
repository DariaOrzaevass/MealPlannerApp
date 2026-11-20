using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Planning.Solvers;
using Microsoft.Extensions.Options;

namespace MealPlanner.Application.MealPlans;

// Creates a solver instance based of the configured settings.
// Solvers are created once and reused later, because they don't have
// mutable state and only store configuration values in readonly fields,
// while all of the working state exists inside a call to IMealPlanSolver.Solve,
// so one solver instance can safely serve concurrent requests.
internal sealed class MealPlanSolverFactory : IMealPlanSolverFactory
{
    private readonly Dictionary<PlannerAlgorithm, IMealPlanSolver> _solvers;

    public MealPlanSolverFactory(IOptions<PlanningOptions> options)
    {
        PlanningOptions settings = options.Value;

        // BacktrackingSolver appears twice: once for Backtracking
        // (first feasible plan) and once for Branch and Bound (the best plan).
        _solvers = new Dictionary<PlannerAlgorithm, IMealPlanSolver>
        {
            [PlannerAlgorithm.Greedy] = new GreedySolver(),
            [PlannerAlgorithm.LayeredGraphDijkstra] =
                new LayeredGraphDijkstraSolver(settings.HeapArity),
            [PlannerAlgorithm.Backtracking] = new BacktrackingSolver(
                timeLimit: settings.TimeLimit,
                nodeLimit: settings.NodeLimit),
            [PlannerAlgorithm.BranchAndBound] = new BacktrackingSolver(
                timeLimit: settings.TimeLimit,
                nodeLimit: settings.NodeLimit,
                optimize: true,
                warmStart: true)
        };
    }

    public IReadOnlyCollection<PlannerAlgorithm> Available => _solvers.Keys;

    public Result<IMealPlanSolver> Create(PlannerAlgorithm algorithm) =>
        _solvers.TryGetValue(algorithm, out IMealPlanSolver? solver)
            ? Result.Success(solver)
            : Result.Failure<IMealPlanSolver>(PlanningErrors.UnknownAlgorithm);
}
