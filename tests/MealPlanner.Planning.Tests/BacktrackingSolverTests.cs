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

    // Checks that BacktrackingSolver proves infeasibility when the budget is too small
    [Fact]
    public void Solve_Should_ProveInfeasibility_WhenBudgetIsTooSmall()
    {
        // Creates and solves a problem with an insufficient budget
        PlanningProblem problem = Problem(dishCount: 40, budgetMinorUnits: 60_000, seed: 2);
        PlanSolution solution = new BacktrackingSolver().Solve(problem);

        // Checks that the solver proved that no feasible solution exists
        solution.Status.Should().Be(PlanningStatus.Infeasible);
        solution.FailureReason.Should().Contain("No feasible solution");
    }

    // Checks that BacktrackingSolver proves infeasibility when the repeat limits is too strict
    [Fact]
    public void Solve_Should_ProveInfeasibility_WhenRepeatLimitIsTooStrict()
    {
        // Creates a catalog with only 3 Lunch dishes
        DishCatalog catalog = DishCatalogBuilder.Build(
        [
            CatalogFactory.Dish(suitableFor: MealKinds.Lunch, ingredientIds: [0]),
            CatalogFactory.Dish(suitableFor: MealKinds.Lunch, ingredientIds: [1]),
            CatalogFactory.Dish(suitableFor: MealKinds.Lunch, ingredientIds: [2])
        ]);

        // Creates a problem with 7 days of Lunches, but the repeat limit is 1
        PlanningProblem problem = PlanningProblem.Create(
            catalog,
            PlanningSpec.Create(
                budgetMinorUnits: 10_000_000,
                durationDays: 7,
                mealsPerDay: [MealKinds.Lunch],
                maxRepeats: 1),
            seed: 1);

        // Runs the backtracking solver
        PlanSolution solution = new BacktrackingSolver().Solve(problem);

        // No feasible solution should exist, since there are
        // only 3 dishes that cannot be repeated and 7 slots to fill.
        solution.Status.Should().Be(PlanningStatus.Infeasible);
    }

    // Checks that backtraking can find solution, when greedy fails to do so.
    [Fact]
    public void Solve_Should_FindPlan_WhereGreedyFails()
    {
        // Creates a problem that greedy cannot solve
        PlanningProblem problem = Problem(dishCount: 60, budgetMinorUnits: 260_000, seed: 6);

        // Runs the greedy solver and the backtracking solver on the same problem
        PlanSolution greedy = new GreedySolver().Solve(problem);
        PlanSolution exact = new BacktrackingSolver().Solve(problem);

        // The test is relevant only when greedy fails to find a solution
        if (greedy.IsSolved)
        {
            return;
        }

        // Checks that backtracking successfully created
        // a meal plan and no constraints were violated
        exact.IsSolved.Should().BeTrue(exact.FailureReason);
        ConstraintValidator.Validate(problem, [.. exact.Assignment])
            .Should().Be(ConstraintViolation.None);
    }

    // Checks that the solver respects the time limit and doesn't run indefinitely.
    [Fact]
    public void Solve_Should_RespectTimeLimit()
    {
        // Creates a problem that is hard to solve
        PlanningProblem problem = Problem(
            dishCount: 400,
            budgetMinorUnits: 330_000,
            maxRepeats: 2,
            minDaysBetweenRepeats: 3,
            minDailyCalories: 2400,
            maxDailyCalories: 2500,
            seed: 12);

        // Creates a solver with a time limit of 300 milliseconds
        var solver = new BacktrackingSolver(timeLimit: TimeSpan.FromMilliseconds(300));

        // Runs the backtracking solver
        PlanSolution solution = solver.Solve(problem);

        // Checks that the solver doesn't exceeded the larger upper time limit of 2 seconds
        solution.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));

        // Checks that if solution wasn't found, no partial assignment was returned
        if (!solution.IsSolved)
        {
            solution.Assignment.Should().BeEmpty();
        }
    }
}
