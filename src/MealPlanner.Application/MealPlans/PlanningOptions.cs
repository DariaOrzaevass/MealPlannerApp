using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Application.MealPlans;

// Settings used for meal plan generation.
// Limits are here for easily experementing with them.
// Changing limits will change results of branch and bound solver.
public sealed class PlanningOptions
{
    public const string SectionName = "Planning";

    // Time limit for the work of the solver (NFR2)
    public TimeSpan TimeLimit { get; init; } = TimeSpan.FromSeconds(2);

    // Maximum number of explore nodes. Second safeguard,
    // does not depend on machine performance, makes
    // solver behavior more reproducible.
    public long NodeLimit { get; init; } = 20_000_000;

    // Initial seed for the random number generator,
    // for reproducibility of solvers results
    public int Seed { get; init; }

    // Heap arity for Dijkstra
    public int HeapArity { get; init; } = 4;

    // If user hasn't chosen the algorithm, the default one is Backtracking
    public string DefaultAlgorithm { get; init; } = "Backtracking";
}
