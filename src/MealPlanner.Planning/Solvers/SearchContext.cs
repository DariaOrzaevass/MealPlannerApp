using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Scoring;

namespace MealPlanner.Planning.Solvers;

// Working environment for Backtracking and Branch and Bound
internal sealed class SearchContext
{
    // Which heuristic flags are enabled
    private readonly BacktrackingHeuristics _heuristics;

    // Which meal slots are assigned
    private readonly bool[] _assigned;

    // Stores ordered domains of candidate dishes,
    // can be null if no candidates
    private readonly int[]?[] _orderedDomains;

    // How many candidate dishes are feasible for each slot.
    // Recalculated after changes if some heuristic needs it
    private readonly int[] _feasibleCount;

    // Search limits: by time and explored nodes
    private readonly long _startTicks;
    private readonly long _tickLimit;
    private readonly long _nodeLimit;

    // Allows to cancell the serach from outside
    private readonly CancellationToken _cancellationToken;

    // Is the _feasibleCount still reflects
    // the the current state of the search
    private bool _countsFresh;

    public SearchContext(
        PlanningProblem problem,
        BacktrackingHeuristics heuristics,
        TimeSpan timeLimit,
        long nodeLimit,
        long startTicks,
        bool optimize,
        CancellationToken cancellationToken)
    {
        Problem = problem;
        State = new SearchState(problem);
        Optimize = optimize;

        _heuristics = heuristics;
        _assigned = new bool[problem.SlotCount];
        _feasibleCount = new int[problem.SlotCount];
        _startTicks = startTicks;
        _tickLimit = (long)(timeLimit.TotalSeconds * Stopwatch.Frequency);
        _nodeLimit = nodeLimit;
        _cancellationToken = cancellationToken;

        _orderedDomains = BuildOrderedDomains(problem, heuristics);
    }

    public PlanningProblem Problem { get; }

    public SearchState State { get; }

    // How many nodes were explored
    public long ExploredNodes { get; set; }

    // Is the limit of time or explored nodes reached
    public bool LimitReached { get; private set; }

    // If false - Backtracking, looks for the first feasible
    // solution and stops. If true - Branch and Bound, keeps
    // searching, pruning the branches where the objective function
    // cannot be improved comparing to the best plan found
    public bool Optimize { get; }

    // Best complete plan found so far. Null if nothing was found
    public int[]? BestAssignment { get; private set; }

    // The value of Objective function of the best plan found
    public double BestObjective { get; private set; } = double.PositiveInfinity;

    // How many times the current best solution was improved
    public int ImprovementCount { get; private set; }

    // Provides a "warm start", starting from the feasible
    // solution, found by another solver like Greedy
    public void SeedBest(int[] assignment, double objective)
    {
        if (objective >= BestObjective)
        {
            return;
        }

        // Branch and Bound can start with this solution
        BestAssignment = assignment;
        BestObjective = objective;
    }

    // Record the solution if its objective is better than the current best assignment
    public bool RecordSolution()
    {
        // Calculates objective of the finished plan
        double objective = ObjectiveFunction.Evaluate(Problem, State.Assignment);

        // Compares it to the best one so far
        if (objective >= BestObjective)
        {
            return false;
        }

        // If it is better - stores it
        BestObjective = objective;

        // Copy here because State.Assignment will continue to change
        BestAssignment = [.. State.Assignment];
        ImprovementCount++;

        return true;
    }

    // Calculates the lower bound on the objective value a completion of the current
    // plan can achieve. Deliberately uses optimistic assumptions - even if
    // optimistic value doesn't beat the current best, the branch can be pruned
    public double LowerBoundObjective()
    {
        ObjectiveWeights weights = Problem.Weights;
        int slotCount = Problem.SlotCount;
        int unassigned = slotCount - State.AssignedCount;

        // Assumes that every remaining slot will get the cheapest dish
        double costTerm = (double)(State.SpentMinorUnits + State.RemainingMinCost)
            / Problem.Spec.BudgetMinorUnits;

        // Only similarities of already assigned neighbouring dishes are considered.
        double monotonyTerm = slotCount > 1 ? State.MonotonySum / (slotCount - 1) : 0;

        // Only already included preference penalties are considered
        double preferenceTerm = State.PreferenceSum / slotCount;

        // Assumes that remaining slots will only use new dishes
        int bestPossibleDistinct = Math.Min(slotCount, State.DistinctDishCount + unassigned);
        double varietyTerm = 1.0 - ((double)bestPossibleDistinct / slotCount);

        // Returns the optimistic objective
        return (weights.Cost * costTerm)
            + (weights.Monotony * monotonyTerm)
            + (weights.Preference * preferenceTerm)
            + (weights.Variety * varietyTerm);
    }

    // Pruning decision of branch and bound, when the branch cannot give a better plan
    // than recorded. Returns true in any of these cases: it is a backtracking, there
    // is no best solution yet, and the branch can still become better than the best plan
    public bool CanImproveOnBest() =>
    !Optimize || BestAssignment is null || LowerBoundObjective() < BestObjective;

    // Marks the slot as assigned and sets _countsFresh to false
    // to show that feasible-domains counts may become outdated
    public void MarkAssigned(int slotIndex)
    {
        _assigned[slotIndex] = true;
        _countsFresh = false;
    }

    // Marks the slot as unassigned and sets _countsFresh to false
    // to show that feasible-domains counts may become outdated
    public void UnmarkAssigned(int slotIndex)
    {
        _assigned[slotIndex] = false;
        _countsFresh = false;
    }

    // Chooses the next slot to assign
    public int SelectSlot()
    {
        // If MRV is disabled then the slots are scanned in
        // the schedule order to return the first unassigned one
        if (!_heuristics.HasFlag(BacktrackingHeuristics.MinimumRemainingValues))
        {
            for (int slot = 0; slot < _assigned.Length; slot++)
            {
                if (!_assigned[slot])
                {
                    return slot;
                }
            }

            return -1;
        }

        // Updates feasible counts if they are outdated
        EnsureCounts();

        int best = -1;
        int bestCount = int.MaxValue;

        for (int slot = 0; slot < _assigned.Length; slot++)
        {
            if (_assigned[slot])
            {
                continue;
            }

            // Selects the slot with the smallest domain of
            // feasible dishes. If two slots have same number
            // of dishes, the one with the smallest index wins.
            if (_feasibleCount[slot] < bestCount)
            {
                bestCount = _feasibleCount[slot];
                best = slot;
            }
        }

        // Returns the slot with the smallest domain 
        return best;
    }

    // Returns candidate dishes in the order the solver should try them
    // If the slot is pinned, its domain contains only the pinned dish.
    // Ordered domains are built for meal kinds and do not know that
    public ReadOnlySpan<int> OrderedDomain(SlotDescriptor slot) =>
    Problem.Spec.IsPinned(slot.Index)
        ? Problem.Domains.ForSlot(slot)
        : _orderedDomains[MealKindIndex.ToIndex(slot.Kind)] ?? Problem.Domains.ForSlot(slot);

    // Pruning by the lowest budget bound. If even the cheapest dishes
    // for the remaining slots cannot exceeds the budget - prune the branch
    public bool FitsBudgetBound(SlotDescriptor slot, int dish)
    {
        // If BudgetBound heuristic is disabled, returns true
        // and candidate is not rejected by this method
        if (!_heuristics.HasFlag(BacktrackingHeuristics.BudgetBound))
        {
            return true;
        }

        // Otherwise, calculates cost lower bound adding the spent budget
        // to dish cost and minimum cost of remaining dishes. Subtracts
        // MinCostForKind, because it includes the current slot
        int projected = State.SpentMinorUnits
            + Problem.Catalog.CostMinorUnits[dish]
            + State.RemainingMinCost
            - Problem.Domains.MinCostForKind(slot.Kind);

        // Returns whether the calculated lower bound is less than the budget
        return projected <= Problem.Spec.BudgetMinorUnits;
    }

    // Checks that every unassigned slot has at least one
    // feasible dish left. If not - the branch is pruned.
    public bool ForwardCheckPasses()
    {
        // If ForwardChecking heuristic is dasabled, nothing happens
        if (!_heuristics.HasFlag(BacktrackingHeuristics.ForwardChecking))
        {
            return true;
        }

        return ComputeCounts();
    }

    // Pruning by the daily calorie range. Given the assigned to this
    // day meal slots and possible calorie ranges of the empty slots,
    // is it possible to finish this day in the required calorie range.
    public bool DayCaloriesStillReachable(int day)
    {
        PlanningSpec spec = Problem.Spec;

        // If there is no calorie restriction, the branch is not pruned
        if (spec.MinDailyCalories <= 0 && spec.MaxDailyCalories == int.MaxValue)
        {
            return true;
        }

        // Gets the current calories of the day
        int current = State.CaloriesOfDay(day);

        // Calculate the minimum and maximum possible calories of remaining slots
        int minRest = 0;
        int maxRest = 0;

        foreach (SlotDescriptor slot in spec.Slots)
        {
            // Skips the already assigned slots and slots of other days
            if (slot.Day != day || _assigned[slot.Index])
            {
                continue;
            }

            // Adds minimum and maximum calories of the meal kind
            minRest += Problem.Domains.MinCaloriesForKind(slot.Kind);
            maxRest += Problem.Domains.MaxCaloriesForKind(slot.Kind);
        }

        // If there is a lower bound on daily calories,
        // checks that most caloric dishes for remaining slots can reach it
        if (spec.MinDailyCalories > 0 && current + maxRest < spec.MinDailyCalories)
        {
            return false;
        }

        // If there is an upper bound on daily calories,
        // checks that least caloric dishes for remaining slots don't exceed it.
        return spec.MaxDailyCalories == int.MaxValue
            || current + minRest <= spec.MaxDailyCalories;
    }

    // Final check of the complete plan. Since heuristic methods are optimized for speed,
    // the check by the full validator ensures that all constraints are satisfied.
    public bool SatisfiesFinalChecks() =>
    ConstraintValidator.Validate(Problem, State.Assignment) == ConstraintViolation.None;

    // Checks whether the time or node limit is reached,
    // or is the search cancelled from outside.
    public bool ShouldStop()
    {
        // Avoids recalculating limits if they were already reached.
        if (LimitReached)
        {
            return true;
        }

        // Handles the external cancellation requests.
        _cancellationToken.ThrowIfCancellationRequested();

        // Checks if explored nodes reached the limit 
        if (ExploredNodes >= _nodeLimit)
        {
            LimitReached = true;

            return true;
        }

        // Checks if the time limit is reached. Only performed once in 1024 nodes,
        // to avoid the overhead of calling Stopwatch.GetTimestamp() for every node.
        if ((ExploredNodes & 1023) == 0 && Stopwatch.GetTimestamp() - _startTicks > _tickLimit)
        {
            LimitReached = true;

            return true;
        }

        return false;
    }

    // Recomputes only if _countsFresh is false
    private void EnsureCounts()
    {
        if (!_countsFresh)
        {
            ComputeCounts();
        }
    }

    // Computes how many feasible dishes are available for each slot
    private bool ComputeCounts()
    {
        // Represent whether this branch is still solvable
        bool alive = true;

        for (int slotIndex = 0; slotIndex < _assigned.Length; slotIndex++)
        {
            // If slot is assigned, no need to calculate candidates for it
            if (_assigned[slotIndex])
            {
                continue;
            }

            // Retrives the information about the slot
            SlotDescriptor slot = Problem.Spec.Slots[slotIndex];
            int count = 0;

            // Counts all the candidate dishes for the slot
            foreach (int dish in Problem.Domains.ForSlot(slot))
            {
                if (State.CanPlace(slot, dish))
                {
                    count++;
                }
            }

            _feasibleCount[slotIndex] = count;

            // Returns false if at least one
            // slot has no feaible dishes left
            if (count == 0)
            {
                alive = false;
            }
        }

        // Now _feasibleCount reflects the the current
        // state of search, so _countsFresh becomes true
        _countsFresh = true;

        return alive;
    }

    // Prepares candidate ordering by the greedy scoring for ValueOrdering.
    // Best candidates considered first. Score is static and calculated only once,
    // to not to introduce many recalculations. Doesn't depend on the search state
    private static int[]?[] BuildOrderedDomains(
        PlanningProblem problem,
        BacktrackingHeuristics heuristics)
    {
        var ordered = new int[]?[MealKindIndex.Count];

        // If ValueOrdering is not enabled the array stays empty
        if (!heuristics.HasFlag(BacktrackingHeuristics.ValueOrdering))
        {
            return ordered;
        }

        foreach (MealKinds kind in problem.Spec.MealsPerDay)
        {
            // Converts Meal Kind to index
            int kindIndex = MealKindIndex.ToIndex(kind);

            // Once a Meal Kind has anordered domain method skips it
            if (ordered[kindIndex] is not null)
            {
                continue;
            }

            // Creates an array of valid dishes for this meal kind
            int[] domain = [.. problem.Domains.ForKind(kind)];

            // If there are no candidate dishes
            // the empty domain is just stored
            if (domain.Length == 0)
            {
                ordered[kindIndex] = domain;

                continue;
            }

            int maxCost = 1;

            // Finds the most expensive dish in the domain
            foreach (int dish in domain)
            {
                maxCost = Math.Max(maxCost, problem.Catalog.CostMinorUnits[dish]);
            }

            var scores = new double[domain.Length];

            // Calculates greedy score consisting of cost and preference
            for (int i = 0; i < domain.Length; i++)
            {
                int dish = domain[i];

                scores[i] = (problem.Weights.Cost
                        * (1.0 - ((double)problem.Catalog.CostMinorUnits[dish] / maxCost)))
                    + (problem.Weights.Preference * (1.0 - problem.PreferencePenalty(dish)));
            }

            var order = new int[domain.Length];

            for (int i = 0; i < order.Length; i++)
            {
                order[i] = i;
            }

            // The copy of indices is sorted, so that
            // when dishes have the same score the one
            // with smallest dish ID will come first
            Array.Sort(order, (left, right) =>
            {
                // Higher scores come first
                int byScore = scores[right].CompareTo(scores[left]);

                return byScore != 0 ? byScore : domain[left].CompareTo(domain[right]);
            });

            var result = new int[domain.Length];

            // Creates the final array of dish IDs
            for (int i = 0; i < order.Length; i++)
            {
                result[i] = domain[order[i]];
            }

            // Stores result by meal kind
            ordered[kindIndex] = result;
        }

        // Returns ordered array
        return ordered;
    }
}
