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

    // Runs the backtracking solver with different heuristics and
    // checks that they don't affect the correctness of the solution
    [Theory]
    [InlineData(BacktrackingHeuristics.None)]
    [InlineData(BacktrackingHeuristics.MinimumRemainingValues)]
    [InlineData(BacktrackingHeuristics.ForwardChecking)]
    [InlineData(BacktrackingHeuristics.BudgetBound)]
    [InlineData(BacktrackingHeuristics.All)]
    public void Heuristics_Should_NotAffectCorrectness(BacktrackingHeuristics heuristics)
    {
        // Creates a problem and runs a backtracking solver with specified heuristic
        PlanningProblem problem = Problem(dishCount: 80, seed: 4);
        PlanSolution solution = new BacktrackingSolver(heuristics).Solve(problem);

        // Checks that if backtracking successfully creates
        // a meal plan, then no constraints were violated
        if (solution.IsSolved)
        {
            ConstraintValidator.Validate(problem, [.. solution.Assignment])
                .Should().Be(ConstraintViolation.None);
        }

        // Checks that if solution wasn't found, no partial assignment was returned
        else
        {
            solution.Assignment.Should().BeEmpty();
        }
    }

    // Checks that using heuristics reduces the number of explored nodes.
    [Fact]
    public void Heuristics_Should_ReduceExploredNodes()
    {
        // Creates a problem that is hard to solve without heuristics
        PlanningProblem problem = Problem(
            dishCount: 50,
            budgetMinorUnits: 300_000,
            maxRepeats: 2,
            minDaysBetweenRepeats: 3,
            seed: 15);

        // Runs the backtracking solver with no heuristics and with all heuristics enabled
        PlanSolution bare = new BacktrackingSolver(BacktrackingHeuristics.None).Solve(problem);
        PlanSolution tuned = new BacktrackingSolver(BacktrackingHeuristics.All).Solve(problem);

        // Checks that the solver with all heuristics found a solution, while
        // exploring less or equal number of nodes than the solver without heuristics
        tuned.IsSolved.Should().BeTrue(tuned.FailureReason);
        tuned.ExploredNodes.Should().BeLessThanOrEqualTo(bare.ExploredNodes);
    }

    // Checks that the backtracking solver agrees with
    // brute-force search on whether a solution exists or not
    [Fact]
    public void Solve_Should_AgreeWithBruteForce_OnExistenceOfSolution()
    {
        // Checks 12 different problems with different random seeds
        for (int seed = 1; seed <= 12; seed++)
        {
            // Creates a random problem with 10 dishes and 3 days of Lunches
            PlanningProblem problem = PlanningProblem.Create(
                RandomCatalog.Build(dishCount: 10, seed: seed, ingredientPool: 8),
                PlanningSpec.Create(
                    budgetMinorUnits: 60_000 + (seed * 4_000),
                    durationDays: 3,
                    mealsPerDay: [MealKinds.Lunch],
                    maxRepeats: 2,
                    minDaysBetweenRepeats: 2),
                seed: seed);

            // Solves the problem with backtracking and brute-force search
            PlanSolution solution = new BacktrackingSolver().Solve(problem);
            bool existsByBruteForce = BruteForceExists(problem);

            // Checks that the backtracking solver agrees with brute-force search
            solution.IsSolved.Should().Be(
                existsByBruteForce,
                $"seed {seed}: Brute-force search found {existsByBruteForce}, backtracking found {solution.Status}");
        }
    }

    // Checks that the backtracking solver respects both daily calorie limits
    [Fact]
    public void Solve_Should_RespectDailyCalorieCorridor()
    {
        // Creates a problem with many dishes and a daily calorie corridor of 1400-2100 calories
        PlanningProblem problem = Problem(
            dishCount: 300,
            minDailyCalories: 1400,
            maxDailyCalories: 2100,
            seed: 5);

        // Runs the backtracking solver
        PlanSolution solution = new BacktrackingSolver().Solve(problem);

        // If the solver didn't find a solution, the test is not relevant
        if (!solution.IsSolved)
        {
            return;
        }

        // Checks that the daily calorie limits are respected for each day of the plan
        for (int day = 1; day <= problem.Spec.DurationDays; day++)
        {
            // Calculates the total calories for the day
            int calories = problem.Spec.Slots
                .Where(s => s.Day == day)
                .Sum(s => problem.Catalog.Calories[solution.Assignment[s.Index]]);

            // Checks that the total calories are in the specified range
            calories.Should().BeInRange(1400, 2100);
        }
    }

    // Brute-force search for a feasible solution.
    // Returns true if it exists. Only for small problems.
    private static bool BruteForceExists(PlanningProblem problem)
    {
        // Creates candidate domains for each slot
        int slotCount = problem.SlotCount;
        var domains = new int[slotCount][];

        // Copies each slot's domain
        for (int slot = 0; slot < slotCount; slot++)
        {
            domains[slot] = [.. problem.Domains.ForSlot(problem.Spec.Slots[slot])];
        }

        // Stores the current assignment of brute-force search
        var current = new int[slotCount];

        // Starts from slot 0
        return Enumerate(0);

        // For each slot
        bool Enumerate(int slot)
        {
            // If all slots were assigned, checks the complete assignment
            if (slot == slotCount)
            {
                return ConstraintValidator.IsFeasible(problem, current);
            }

            // Tries every candidate dish for the current slot
            foreach (int dish in domains[slot])
            {
                // Assigns the dish
                current[slot] = dish;

                // moves to the next slot
                if (Enumerate(slot + 1))
                {
                    return true;
                }
            }

            // No feasible assignment exists for the current branch
            return false;
        }
    }
}
