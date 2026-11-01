using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Solvers;

public sealed class BacktrackingSolver : IMealPlanSolver
{
    // Stores which heuristics are enabled.
    private readonly BacktrackingHeuristics _heuristics;

    // Stores time and node limits for the search.
    private readonly TimeSpan _timeLimit;
    private readonly long _nodeLimit;

    // Decides whether it is a backtracking or branch and bound.
    private readonly bool _optimize;

    // Decides whether to start with a greedy initial solution or not.
    private readonly bool _warmStart;

    public BacktrackingSolver(
        BacktrackingHeuristics heuristics = BacktrackingHeuristics.Recommended,
        TimeSpan? timeLimit = null,
        long nodeLimit = 20_000_000,
        bool optimize = false,
        bool warmStart = true)
    {
        _heuristics = heuristics;

        // Uses caller's time limit if provided or default 2 seconds.
        _timeLimit = timeLimit ?? TimeSpan.FromSeconds(2);
        _nodeLimit = nodeLimit;
        _optimize = optimize;
        _warmStart = warmStart;
    }

    // Returns the name of the algorithm, depending on heurristics and _optimize
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

        // Creates a new search context that stores all information needed during the recursive search.
        var context = new SearchContext(
            problem, _heuristics, _timeLimit, _nodeLimit, start, _optimize, cancellationToken);

        // Only used for branch and bound. If the greedy solution is
        // feasible, it is used as initial best solution to prune branches.
        if (_optimize && _warmStart)
        {
            WarmStart(problem, context, cancellationToken);
        }

        // Starts with empty assignment and recursively searches for a feasible solution.
        Search(context);

        // Measures how much time the previous actions took
        long elapsed = Stopwatch.GetTimestamp() - start;

        // If a feasible solution was found, for backtracking it's
        // the first one, for branch and bound - the best one.
        if (context.BestAssignment is not null)
        {
            int[] assignment = context.BestAssignment;

            // The total cost of the plan is calculated
            int totalCost = 0;

            foreach (int dish in assignment)
            {
                totalCost += problem.Catalog.CostMinorUnits[dish];
            }

            // Returns the successfull result. The solution is proven
            // optimal only if the search explored the whole tree
            return PlanSolution.Solved(
                assignment,
                totalCost,
                context.BestObjective,
                context.ExploredNodes,
                elapsed,
                isProvenOptimal: _optimize && !context.LimitReached);
        }

        // The meal plan was not completed, the search was stopped due to time or node limit.
        // It is not determined whether a solution exists or not, therefore Exhausted is returned.
        if (context.LimitReached)
        {
            return PlanSolution.Exhausted(
                $"The search limit was exhausted after exploring {context.ExploredNodes} nodes:"
                + " The existance of the feasible solution cannot be determined.",
                context.ExploredNodes,
                elapsed);
        }

        // The tree was fully explored, but no feasible solution was found.
        return PlanSolution.Infeasible(
            $"An exhaustive search ({context.ExploredNodes} nodes) proved:"
            + "No feasible solution satisfying all constraints exists.",
            context.ExploredNodes,
            elapsed);
    }

    // Starts with the best plan that was found with the greedy
    // algorithm, if it was feasible. Used by branch and bound to
    // prune branches that have worse objective than the best plan.
    private static void WarmStart(
        PlanningProblem problem,
        SearchContext context,
        CancellationToken cancellationToken)
    {
        // Runs the greedy algorithm
        PlanSolution greedy = new GreedySolver().Solve(problem, cancellationToken);

        // If the greedy algorithm successfully found feasible solution,
        // passes it to the search context as the initial best plan.
        if (greedy.IsSolved)
        {
            context.SeedBest([.. greedy.Assignment], greedy.Objective);
        }
    }

    // Recursive depth-first search for solution.
    // Returns true if the search should stop.
    // For backtracking it stops when the first feasible solution is found.
    // For branch and bound - when any limit is reached.
    private static bool Search(SearchContext context)
    {
        // If the meal plan is complete
        if (context.State.IsComplete)
        {
            // Checks that plan satisfies all constraints,
            // budget, for example, if BudgetBound wasn't used
            if (!context.SatisfiesFinalChecks())
            {
                return false;
            }

            // Records the solution, if it is better than the best one found so far.
            context.RecordSolution();

            // For backtracking, stops after the first solution found.
            // For branch and bound, continues looking for a better one.
            return !context.Optimize;
        }

        // The node or time limit was reached,
        // or cancellation was requested.
        if (context.ShouldStop())
        {
            return false;
        }

        return false;
    }
}
