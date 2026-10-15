using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Solvers;

// The Dijkstra's Algorithm
public sealed class LayeredGraphDijkstraSolver : IMealPlanSolver
{
    private readonly int _heapArity;

    public LayeredGraphDijkstraSolver(int heapArity = 4) => _heapArity = heapArity;

    // the name of the algorithm
    public string Name => "Dijkstra (Layered Graph)";

    public PlanSolution Solve(PlanningProblem problem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);

        long start = Stopwatch.GetTimestamp();

        if (problem.Domains.HasEmptyDomain)
        {
            return PlanSolution.Infeasible(
                "There is no available dish for one of the meals.",
                exploredNodes: 0,
                Stopwatch.GetTimestamp() - start);
        }

        // Builds the layered graph based on the problem
        LayeredGraph graph = LayeredGraph.Build(problem);
        LayeredShortestPath.Result path = LayeredShortestPath.Run(graph, _heapArity, cancellationToken);

        long elapsed = Stopwatch.GetTimestamp() - start;

        // If the path was not found there is no solution
        if (!path.Found)
        {
            return PlanSolution.Infeasible(
                "There is no path in the layered graph. ",
                path.ExploredNodes,
                elapsed);
        }

        ConstraintViolation violation = ConstraintValidator.Validate(problem, path.Assignment);

        if (violation != ConstraintViolation.None)
        {
            return PlanSolution.Exhausted(
                $"The shortest path violates the constraint that cannot be expressed by weight: {violation}.",
                path.ExploredNodes,
                elapsed);
        }

        int totalCost = 0;

        // Total cost
        foreach (int dish in path.Assignment)
        {
            totalCost += problem.Catalog.CostMinorUnits[dish];
        }

        // delete later (to be able to compile)
        return PlanSolution.Infeasible(
            "There is no available dish for one of the meals.",
            exploredNodes: 0,
            Stopwatch.GetTimestamp() - start);
    }
}
