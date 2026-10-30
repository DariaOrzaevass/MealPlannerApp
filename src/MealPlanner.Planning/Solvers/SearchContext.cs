using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MealPlanner.Planning.Models;

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
    //private readonly int[]?[] _orderedDomains;

    // How many candidate dishes are feasible for each slot.
    // Recalculated after changes if some heuristic needs it
    private readonly int[] _feasibleCount;

    // Search limits: by time and explored nodes
    private readonly long _startTicks;
    private readonly long _tickLimit;
    private readonly long _nodeLimit;

    // Allows to cancell the serach from outside
    private readonly CancellationToken _cancellationToken;

    // Is the _feasibleCount still reslects
    // the the current state of the search
    //private bool _countsFresh;

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

        //_orderedDomains = ;
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
}
