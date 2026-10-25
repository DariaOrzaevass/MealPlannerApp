using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Scoring;

namespace MealPlanner.Planning.Solvers;

// The Dijkstra's Algorithm Solver
public sealed class LayeredGraphDijkstraSolver : IMealPlanSolver
{
    // How many children each node can have
    private readonly int _heapArity;

    public LayeredGraphDijkstraSolver(int heapArity = 4) => _heapArity = heapArity;

    // The name of the algorithm
    public string Name => "Dijkstra (Layered Graph)";

    // The main solving method that takes a problem and returns a plan solution
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

        // Builds the layered graph based on the problem
        LayeredGraph graph = LayeredGraph.Build(problem);

        // Runs the actual Dijkstra's Algorithm
        LayeredShortestPath.Result path = LayeredShortestPath.Run(graph, _heapArity, cancellationToken);

        // Measures how much time the previous actions took
        long elapsed = Stopwatch.GetTimestamp() - start;

        // If the path was not found there is no solution.
        // The minimum repeat gap restriction could've eliminate all possible transitions
        if (!path.Found)
        {
            return PlanSolution.Infeasible(
                "There is no path in the layered graph.",
                path.ExploredNodes,
                elapsed);
        }

        // Validates that the path satisfies all constraints
        ConstraintViolation violation = ConstraintValidator.Validate(problem, path.Assignment);

        // Shows which first constraint was violated, if it was.
        // Exhausted because Dijkstra didn't prove that plan doesn't exist
        if (violation != ConstraintViolation.None)
        {
            return PlanSolution.Exhausted(
                $"The shortest path violates the constraint that cannot be expressed by weight: {violation}.",
                path.ExploredNodes,
                elapsed);
        }

        int totalCost = 0;

        // Calculates the total cost of the plan
        foreach (int dish in path.Assignment)
        {
            totalCost += problem.Catalog.CostMinorUnits[dish];
        }

        // Returns a successfully solved result
        return PlanSolution.Solved(
            path.Assignment,
            totalCost,
            ObjectiveFunction.Evaluate(problem, path.Assignment),
            path.ExploredNodes,
            elapsed);
    }

    // Calculates a value to use as a lower bound in comparison.
    // Weight of the shortest-path without the variety penalty.
    public double LowerBound(PlanningProblem problem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (problem.Domains.HasEmptyDomain)
        {
            return double.PositiveInfinity;
        }

        // Builds a Layered Graph
        LayeredGraph graph = LayeredGraph.Build(problem);

        // Returns only the weight of the path
        return LayeredShortestPath.Run(graph, _heapArity, cancellationToken).PathWeight;
    }
}
