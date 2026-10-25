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
                "There is no valid dish for one of the meal slots.",
                exploredNodes: 0,
                Stopwatch.GetTimestamp() - start);
        }

        // Creates a new search state
        var state = new SearchState(problem);

        // Algorithm tries to fill every slot, if it does it successfully - the null is returned
        string? fillFailure = Fill(problem, state, cancellationToken);

        // If algorithm failed to fill the plan, it returns the reason why
        if (fillFailure is not null)
        {
            return PlanSolution.Infeasible(fillFailure, 0, Stopwatch.GetTimestamp() - start);
        }

        // Algorithm then tries (if needed) to repair the plan by
        // changing one dish at a time to a cheaper one to fit the budget
        Repair(problem, state, cancellationToken);

        // Validates that the meal plan satisfies all constraints
        ConstraintViolation violation = ConstraintValidator.Validate(problem, state.Assignment);

        // Measures how much time the previous actions took
        long elapsed = Stopwatch.GetTimestamp() - start;

        // The Greedy Algorithm couldn't find a solution fitting all constraints,
        // even though it can exist. Shows which constraint was violated first.
        if (violation != ConstraintViolation.None)
        {
            return PlanSolution.Exhausted(
                $"The Greedy Algorithm violates the constraint: {violation}.",
                exploredNodes: 0,
                elapsed);
        }

        // Returns a successfully solved result
        return PlanSolution.Solved(
            [.. state.Assignment],
            state.SpentMinorUnits,
            ObjectiveFunction.Evaluate(problem, state.Assignment),
            exploredNodes: 0,
            elapsed);
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

    // Tries to fill all the slots or returns the reason why it failed
    private static string? Fill(
        PlanningProblem problem,
        SearchState state,
        CancellationToken cancellationToken)
    {
        // Builds the correct slot order
        int[] order = BuildSlotOrder(problem);

        foreach (int slotIndex in order)
        {
            // Allows to cancel externally
            cancellationToken.ThrowIfCancellationRequested();

            SlotDescriptor slot = problem.Spec.Slots[slotIndex];

            // Firstly, tries to choose with looking ahead and respecting the budget. The
            // dish is chosen only if remaining slots can be filled with cheapest options
            int chosen = ChooseBest(problem, state, slot, respectBudgetLookahead: true);

            // If the first try fail, forgets about budget, so it is exceeded.
            // The repair phase will try to fix that later, making the plan cheaper.
            chosen = chosen >= 0
                ? chosen
                : ChooseBest(problem, state, slot, respectBudgetLookahead: false);

            // Describes why no dish can be chosen
            if (chosen < 0)
            {
                return DescribeDeadEnd(problem, state, slot);
            }

            // Assign a dish to a meal slot
            state.Place(slot, chosen);
        }

        return null;
    }

    // Chooses the best scoring dish for the slot or -1 if none was chosen
    private static int ChooseBest(
        PlanningProblem problem,
        SearchState state,
        SlotDescriptor slot,
        bool respectBudgetLookahead)
    {
        // Gets the candidate dishes for the slot, its budget share,
        // and the minimum amount of money for the remaining slots
        ReadOnlySpan<int> domain = problem.Domains.ForSlot(slot);
        double share = state.BudgetShareFor(slot);
        int lookahead = respectBudgetLookahead ? state.MinCostOfRemainingAfter(slot) : 0;

        int best = -1;
        double bestScore = double.NegativeInfinity;

        foreach (int dish in domain)
        {
            // Checks whether it is possible to place a dish in the slot
            // (maximum amount of repeats, the gap between repeats, and
            // the maximum daily calorie number are not violated)
            if (!state.CanPlace(slot, dish))
            {
                continue;
            }

            // If after adding the cost of the new dish to a spent share of budget, the
            // minimum needed money to finish the plan exceeds the budget - dish is skipped
            if (respectBudgetLookahead &&
                state.SpentMinorUnits + problem.Catalog.CostMinorUnits[dish] + lookahead
                    > problem.Spec.BudgetMinorUnits)
            {
                continue;
            }

            // Gets the heuristic score of the dish
            double score = Score(problem, state, slot, dish, share);

            // If the scores of dishes are equal,
            // wins the one with smaller index
            if (score > bestScore)
            {
                bestScore = score;
                best = dish;
            }
        }

        // Returns the best fitting dish or -1
        return best;
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

    // Describes wich constraint led the GreedySolver to the dead end
    private static string DescribeDeadEnd(PlanningProblem problem, SearchState state, SlotDescriptor slot)
    {
        ReadOnlySpan<int> domain = problem.Domains.ForSlot(slot);

        // Counts for how many candidate dishes
        // were blocked by each constraint
        int blockedByRepeats = 0;
        int blockedByGap = 0;
        int blockedByCalories = 0;

        // Each dish blockes is counted only to the first reason of blockage
        foreach (int dish in domain)
        {
            // How many dishes were blocked by maximum number of repeats
            if (state.UseCount(dish) >= problem.Spec.MaxRepeats)
            {
                blockedByRepeats++;
            }

            // How many dishes were blocked by gap violation
            else if (state.ViolatesGap(slot, dish))
            {
                blockedByGap++;
            }

            // How many dishes were blocked for exceeding daily calories
            else if (state.ExceedsDailyCalories(slot, dish))
            {
                blockedByCalories++;
            }
        }

        // These blocks return because of which constraint the meal plan is not created

        if (blockedByGap >= blockedByRepeats && blockedByGap >= blockedByCalories && blockedByGap > 0)
        {
            return $"Day {slot.Day}: all valid dishes violate the gap between repeats " +
                $"({problem.Spec.MinDaysBetweenRepeats} d.). Lower the gap or expand the catalogue.";
        }

        if (blockedByRepeats >= blockedByCalories && blockedByRepeats > 0)
        {
            return $"Day {slot.Day}: all valid dishes violate the allowed repeat number " +
                $"({problem.Spec.MaxRepeats}). Increase the repeat number or expand the catalogue.";
        }

        if (blockedByCalories > 0)
        {
            return $"Day {slot.Day}: all valid dishes violate the upper limits of calories " +
                $"({problem.Spec.MaxDailyCalories} kcal). Increase the limit or expand the catalogue.";
        }

        return $"Day {slot.Day}: there is no valid dishes for the meal slot.";
    }

    // Repairs already completed plan by reducing its price
    private static void Repair(
        PlanningProblem problem,
        SearchState state,
        CancellationToken cancellationToken)
    {
        // Repair iterations are fixed to 256
        for (int iteration = 0; iteration < _repairIterationLimit; iteration++)
        {
            // If the plan already fits the budget repairs stop
            if (state.SpentMinorUnits <= problem.Spec.BudgetMinorUnits)
            {
                return;
            }

            // Allows to cancel externally
            cancellationToken.ThrowIfCancellationRequested();

            int bestSlot = -1;
            int bestDish = -1;
            double bestGain = 0;

            foreach (SlotDescriptor slot in problem.Spec.Slots)
            {
                // Gets the dish in the slot, its cost, budget
                // share for the slot and score of the dish
                int current = state.Assignment[slot.Index];
                int currentCost = problem.Catalog.CostMinorUnits[current];
                double share = state.BudgetShareFor(slot);
                double currentScore = Score(problem, state, slot, current, share);

                // Loops through all candidate dishes for this slot
                foreach (int candidate in problem.Domains.ForSlot(slot))
                {
                    // Skips the current dish
                    if (candidate == current)
                    {
                        continue;
                    }

                    // Calculates how much the candidate dish would save
                    int saving = currentCost - problem.Catalog.CostMinorUnits[candidate];

                    // If it saves nothing - skips that dish
                    if (saving <= 0)
                    {
                        continue;
                    }

                    // If the dish cannot be placed here
                    // due to constraints - skips it
                    if (!state.CanReplace(slot, candidate))
                    {
                        continue;
                    }

                    // How big is the greedy score loss. Doesn't allow negative if new score better than current
                    double scoreLoss = Math.Max(0.0, currentScore - Score(problem, state, slot, candidate, share));

                    // How much money is saved for loss in Greedy score. If the
                    // replacement has no loss in score adds 0.05 to not to divide by 0
                    double gain = saving / (scoreLoss + 0.05);

                    // Remembers the best slot and a replacement candidate
                    if (gain > bestGain)
                    {
                        bestGain = gain;
                        bestSlot = slot.Index;
                        bestDish = candidate;
                    }
                }
            }

            // If no single replacements will improve
            // the plan, it stops (local minimum)
            if (bestSlot < 0)
            {
                return;
            }

            // Does the best replacement
            state.Replace(problem.Spec.Slots[bestSlot], bestDish);
        }
    }
}
