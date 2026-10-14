using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Solvers;

// Result of the solver algorithm
public sealed class PlanSolution
{
    private PlanSolution(
    PlanningStatus status,
    int[] assignment,
    int totalCostMinorUnits,
    double objective,
    long exploredNodes,
    long elapsedTicks,
    string? failureReason,
    bool isProvenOptimal)
    {
        Status = status;
        IsProvenOptimal = isProvenOptimal;
        Assignment = assignment;
        TotalCostMinorUnits = totalCostMinorUnits;
        Objective = objective;
        ExploredNodes = exploredNodes;
        ElapsedTicks = elapsedTicks;
        FailureReason = failureReason;
    }

    public PlanningStatus Status { get; }

    // Returns whether the plan was solved or not
    public bool IsSolved => Status == PlanningStatus.Solved;

    // Assigned numbers of dishes in the slots. Empty if there is no solution found
    public IReadOnlyList<int> Assignment { get; }

    // Total cost of the plan in minor units (cents)
    public int TotalCostMinorUnits { get; }

    // The value of objective function - the smaller the better
    public double Objective { get; }

    // How many nodes of search tree have been explored
    public long ExploredNodes { get; }

    // For how many ticks the solver run
    public long ElapsedTicks { get; }

    // Why the solution wasn't found. null if solurion was found
    public string? FailureReason { get; }

    // Returns whether it was proven that the plan is optimal. For branch and bound true only
    // if the whole tree was exhausted, if not - a better plan might exist in an unexplored branch
    public bool IsProvenOptimal { get; }

    // Stopwatch.Frequency tells how many ticks happens in 1 second, so Elapsed converts
    // ticks into seconds and then gives the TimeSpan to use seconds or milliseconds later
    public TimeSpan Elapsed => TimeSpan.FromSeconds(
    (double)ElapsedTicks / System.Diagnostics.Stopwatch.Frequency);

    // Measures the diversity of the plan - a proportion of meal slots that has different dishes
    public double DiversityIndex =>
    Assignment.Count == 0 ? 0 : (double)Assignment.Distinct().Count() / Assignment.Count;

    // What fraction of the user's budget was spend
    public double BudgetUsage(PlanningSpec spec) =>
    spec.BudgetMinorUnits == 0 ? 0 : (double)TotalCostMinorUnits / spec.BudgetMinorUnits;

    // Creates a successful solution result
    public static PlanSolution Solved(
    int[] assignment,
    int totalCostMinorUnits,
    double objective,
    long exploredNodes,
    long elapsedTicks,
    bool isProvenOptimal = false) =>
    new(PlanningStatus.Solved, assignment, totalCostMinorUnits, objective,
        exploredNodes, elapsedTicks, failureReason: null, isProvenOptimal);

    // Creates infeasible solution result
    public static PlanSolution Infeasible(string reason, long exploredNodes, long elapsedTicks) =>
    new(PlanningStatus.Infeasible, [], 0, double.PositiveInfinity,
        exploredNodes, elapsedTicks, reason, isProvenOptimal: false);

    // Creates an exhausted solution result
    public static PlanSolution Exhausted(string reason, long exploredNodes, long elapsedTicks) =>
    new(PlanningStatus.Exhausted, [], 0, double.PositiveInfinity,
        exploredNodes, elapsedTicks, reason, isProvenOptimal: false);
}
