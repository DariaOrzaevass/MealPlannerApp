using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Solvers;

// Heusistics that can be used by backtracking algorithm
[Flags]
public enum BacktrackingHeuristics
{
    // No heuristic: slots in original order,
    // candidate values in original order, no pruning
    None = 0,

    // Minimum-remaining-values (MRV) heuristic,
    // reduces the number of explored nodes
    MinimumRemainingValues = 1,

    // After assignment checks that no
    // unassigned slot has an empty domain
    ForwardChecking = 2,

    // Prunes branches using a lower bound on the
    // minimum remaining cost. Cheapest plan for
    // remaining slots must not exceed the budget.
    BudgetBound = 4,

    // Candidate dishes ordering by the greedy scoring function.
    // Helps to find feasible solution faster. It doesn't
    // reduce the tree, when searching for all solutions.
    ValueOrdering = 8,

    // Combines all heuristics
    All = MinimumRemainingValues | ForwardChecking | BudgetBound | ValueOrdering,

    // Recommended combination - all heuristics without MRV,
    // since it can hurt performance of calorie-based pruning, when
    // several slots of the day are connected by the daily calorie range
    Recommended = ForwardChecking | BudgetBound | ValueOrdering
}
