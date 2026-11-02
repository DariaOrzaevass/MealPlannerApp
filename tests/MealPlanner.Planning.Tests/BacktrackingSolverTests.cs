using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Solvers;
using MealPlanner.Planning.Tests.Models;
using FluentAssertions;

namespace MealPlanner.Planning.Tests;

public class BacktrackingSolverTests
{
    // Most tests use this standard schedule
    private static readonly MealKinds[] _threeMeals =
    [MealKinds.Breakfast, MealKinds.Lunch, MealKinds.Dinner];

    // Creates a complete testing PlanningProblem
    private static PlanningProblem Problem(
        int dishCount = 200,
        int seed = 1,
        int budgetMinorUnits = 500_000,
        int durationDays = 7,
        int maxRepeats = 3,
        int minDaysBetweenRepeats = 2,
        int minDailyCalories = 0,
        int maxDailyCalories = int.MaxValue,
        IReadOnlyList<MealKinds>? meals = null) =>
        PlanningProblem.Create(
            RandomCatalog.Build(dishCount, seed),
            PlanningSpec.Create(
                budgetMinorUnits,
                durationDays,
                meals ?? _threeMeals,
                maxRepeats: maxRepeats,
                minDaysBetweenRepeats: minDaysBetweenRepeats,
                minDailyCalories: minDailyCalories,
                maxDailyCalories: maxDailyCalories),
            seed: seed);

    // If backtracking algorithm solved the problem, no constraints should be violated
    [Fact]
    public void Solve_Should_BuildFeasiblePlan()
    {
        // Creates a default problem for 7 days and 3 meals gives 21 slots
        PlanningProblem problem = Problem();

        // Runs the backtracking solver
        PlanSolution solution = new BacktrackingSolver().Solve(problem);

        // Checks that the algorithm successfully created a meal plan
        solution.IsSolved.Should().BeTrue(solution.FailureReason);

        // Checks that no constraints were violated
        ConstraintValidator.Validate(problem, [.. solution.Assignment])
            .Should().Be(ConstraintViolation.None);
    }

    // Checks that the solver actually reports the number of explored
    // nodes, which is the main metric of effectiveness of pruning
    [Fact]
    public void Solve_Should_ReportExploredNodes()
    {
        // Runs the backtracking solver on a default problem
        PlanSolution solution = new BacktrackingSolver().Solve(Problem());

        // The algorithm should've explored at least one
        // node, or it wouldn't be able to find a solution.
        solution.ExploredNodes.Should().BeGreaterThan(0);
    }

    // Checks that BacktrackingSolver provides same results for same planning problem
    [Fact]
    public void Solve_Should_BeDeterministic()
    {
        // Creates and solves the same problem (with the same seed) twice
        PlanSolution first = new BacktrackingSolver().Solve(Problem(seed: 33));
        PlanSolution second = new BacktrackingSolver().Solve(Problem(seed: 33));

        // Checks that the two solutions are identical
        first.Assignment.Should().Equal(second.Assignment);
        first.ExploredNodes.Should().Be(second.ExploredNodes);
    }

}
