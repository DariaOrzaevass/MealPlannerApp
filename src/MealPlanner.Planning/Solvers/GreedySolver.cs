using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Scoring;

namespace MealPlanner.Planning.Solvers;

// The Greedy Algorithm Solver
public sealed class GreedySolver : IMealPlanSolver
{
    // The limits of repair attenpts
    private const int _repairIterationLimit = 256;

    // The name of the algorithm
    public string Name => "Greedy";

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

        // Creates a new search state
        var state = new SearchState(problem);

        // Delete later, only to compile
        return PlanSolution.Infeasible(
            "There is no available dish for one of the meal slots.",
            exploredNodes: 0,
            Stopwatch.GetTimestamp() - start);
    }

    // Builds an order in which slots should be considered
    private static int[] BuildSlotOrder(PlanningProblem problem)
    {
        // Firstly the order array is created with number of slots as its size
        int slotCount = problem.SlotCount;
        var order = new int[slotCount];

        // The array is filled with slot numbers in ascending order [0, 1, 2, ...]
        for (int i = 0; i < slotCount; i++)
        {
            order[i] = i;
        }

        // The array is sorted so that the slots with the smallest
        // candidate domains are placed in the beginning
        Array.Sort(order, (left, right) =>
        {
            int byDomain = problem.Domains.ForSlot(problem.Spec.Slots[left]).Length
                .CompareTo(problem.Domains.ForSlot(problem.Spec.Slots[right]).Length);

            // If slots have the same length of domains,
            // the one with lower index will come up first
            return byDomain != 0 ? byDomain : left.CompareTo(right);
        });

        // Returns the resulting ordered aray of meal slots
        return order;
    }

    // Score of the dish, how good it is for
    // the slot - the core greedy heuristic
    private static double Score(
        PlanningProblem problem,
        SearchState state,
        SlotDescriptor slot,
        int dish,
        double share)
    {
        // Gets weights (Cost, Variety, Monotony, Preference)
        // and the cost of the dish from the Planning problem
        ObjectiveWeights weights = problem.Weights;
        int cost = problem.Catalog.CostMinorUnits[dish];

        // The reward (from 0 to 1) the dish gets from fitting into allocated budget share
        // Becomes 0 if the price of the dish is more than twice as much as budget share
        double costFit = share <= 0
            ? 0.0
            : 1.0 - Math.Min(1.0, cost / (2.0 * share));

        // The reward the dish gets if it wasn't used much
        double novelty = 1.0 / (1 + state.UseCount(dish));

        // The reward the dish gets if it isn't too similar to the previous dish
        // If there is no previous dish the monotony reward is 1
        int previous = slot.Index > 0 ? state.Assignment[slot.Index - 1] : -1;
        double monotony = previous < 0 ? 1.0 : 1.0 - problem.Similarity.Between(previous, dish);

        // The reward the dish gets if it fits preferences of the user
        double preference = 1.0 - problem.PreferencePenalty(dish);

        // Returns the final score of the dish, the higher the better
        return (weights.Cost * costFit)
            + (weights.Variety * novelty)
            + (weights.Monotony * monotony)
            + (weights.Preference * preference);
    }
}
