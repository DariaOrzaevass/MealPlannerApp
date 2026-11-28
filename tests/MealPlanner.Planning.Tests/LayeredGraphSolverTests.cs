using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using MealPlanner.Planning.DataStructures;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Solvers;
using MealPlanner.Planning.Tests.Models;

namespace MealPlanner.Planning.Tests;

// Tests for LayeredGraphDijkstraSolver
public class LayeredGraphDijkstraSolverTests
{
    // Most tests use this standard schedule
    private static readonly MealKinds[] _threeMeals =
       [MealKinds.Breakfast, MealKinds.Lunch, MealKinds.Dinner];

    // Creates a complete testing PlanningProblem
    private static PlanningProblem Problem(
        int dishCount = 120,
        int seed = 1,
        int budgetMinorUnits = 1_000_000,
        int durationDays = 7,
        int maxRepeats = int.MaxValue,
        int minDaysBetweenRepeats = 0,
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
                maxDailyCalories: maxDailyCalories),
            seed: seed);

    // If Dijkstra's algorithm solved the problem, no constraints should be violated
    [Fact]
    public void Dijkstra_Should_BuildFeasiblePlan()
    {
        // Creates a default problem. For 7 days and 3 meals that gives 21 slots
        PlanningProblem problem = Problem();

        // Runs the LayeredGraphDijkstraSolver
        PlanSolution solution = new LayeredGraphDijkstraSolver().Solve(problem);

        // Checks that the algorithm successfully created a meal plan
        solution.IsSolved.Should().BeTrue(solution.FailureReason);

        // Checks that no constraints were violated
        ConstraintValidator.Validate(problem, [.. solution.Assignment])
            .Should().Be(ConstraintViolation.None);
    }

    // Checks that Dijkstra's algorithm chooses only one candidate dish from every layer of the graph
    [Fact]
    public void Dijkstra_Should_VisitEveryLayer()
    {
        PlanningProblem problem = Problem();

        PlanSolution solution = new LayeredGraphDijkstraSolver().Solve(problem);

        // Checks that there are correct number of slots
        solution.Assignment.Should().HaveCount(21);

        // Checks that selected dishes for every slot belong to this slot candidate domain
        for (int slot = 0; slot < solution.Assignment.Count; slot++)
        {
            problem.Domains.ForSlot(problem.Spec.Slots[slot]).ToArray()
                .Should().Contain(solution.Assignment[slot]);
        }
    }

    // Checks that Dijkstra result correctness do not change with different heap arity
    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    public void Dijkstra_Should_BeIndependentOfHeapArity(int arity)
    {
        PlanningProblem problem = Problem(seed: 5);

        // Binary heap used as reference
        PlanSolution reference = new LayeredGraphDijkstraSolver(2).Solve(problem);
        PlanSolution actual = new LayeredGraphDijkstraSolver(arity).Solve(problem);

        PathObjective.Evaluate(problem, actual.Assignment)
            .Should().BeApproximately(PathObjective.Evaluate(problem, reference.Assignment), 1e-9);
    }

    // Checks that LayeredGraphDijkstraSolver provides same results for same planning problem
    [Fact]
    public void Dijkstra_Should_BeDeterministic()
    {
        // Creates and solves the same problem (with the same seed) twice
        PlanSolution first = new LayeredGraphDijkstraSolver().Solve(Problem(seed: 77));
        PlanSolution second = new LayeredGraphDijkstraSolver().Solve(Problem(seed: 77));

        // Checks that the two solutions are identical
        first.Assignment.Should().Equal(second.Assignment);
        first.Objective.Should().Be(second.Objective);
    }

    // Checks that Dijkstra's result is not worse than the Greedy's result according
    // to the Path Objective (objective function without the variety term)
    [Fact]
    public void Dijkstra_Should_BeatGreedy_OnPathObjective()
    {
        PlanningProblem problem = Problem(seed: 3);

        // Solves the same problem with GreedySolver and LayeredGraphDijkstraSolver
        PlanSolution greedy = new GreedySolver().Solve(problem);
        PlanSolution dijkstra = new LayeredGraphDijkstraSolver().Solve(problem);

        greedy.IsSolved.Should().BeTrue(greedy.FailureReason);
        dijkstra.IsSolved.Should().BeTrue(dijkstra.FailureReason);

        // Checks that Dijkstra's result is no worse than Greedy's result
        PathObjective.Evaluate(problem, dijkstra.Assignment)
            .Should().BeLessThanOrEqualTo(PathObjective.Evaluate(problem, greedy.Assignment) + 1e-9);
    }

    // Checks that Dijkstra finds the true minimum of layered-graph
    // objective by comparing it to Brute Force solution
    [Fact]
    public void Dijkstra_Should_MatchBruteForce_OnTinyProblem()
    {
        // Creates a small problem
        PlanningProblem problem = PlanningProblem.Create(
            RandomCatalog.Build(dishCount: 14, seed: 99, ingredientPool: 10),
            PlanningSpec.Create(
                budgetMinorUnits: 10_000_000,
                durationDays: 2,
                mealsPerDay: [MealKinds.Breakfast, MealKinds.Lunch]),
            seed: 99);

        // Solves the problem with LayeredGraphDijkstraSolver
        PlanSolution solution = new LayeredGraphDijkstraSolver().Solve(problem);

        solution.IsSolved.Should().BeTrue(solution.FailureReason);

        // Brute Force reference solution
        double best = BruteForceMinimum(problem);

        // Checks that Dijkstra gets the same minimum value as exhaustive search
        PathObjective.Evaluate(problem, solution.Assignment).Should().BeApproximately(best, 1e-9);
    }

    // Checks that Dijkstra correctly reports Infeasibility in case of Empty Domain
    [Fact]
    public void Dijkstra_WithEmptyDomain_Should_ReportInfeasible()
    {
        // Very restrictive problem
        PlanningProblem problem = PlanningProblem.Create(
            RandomCatalog.Build(60, seed: 4),
            PlanningSpec.Create(
                1_000_000, 7, _threeMeals,
                dietLevel: 2,
                excludedAllergens: AllergenMask.FromPositions([0, 1, 2, 3, 4, 5, 6, 7, 8]),
                maxCookingTimeMinutes: 6),
            seed: 4);

        PlanSolution solution = new LayeredGraphDijkstraSolver().Solve(problem);

        solution.Status.Should().Be(PlanningStatus.Infeasible);
    }

    // Checks that LayeredGraphDijkstraSolver proves infeasibility when the budget is too small
    [Fact]
    public void Dijkstra_WithTinyBudget_Should_NotReturnInvalidPlan()
    {
        // Creates and solves a problem with an insufficient budget
        PlanningProblem problem = Problem(budgetMinorUnits: 40_000);
        PlanSolution solution = new LayeredGraphDijkstraSolver().Solve(problem);

        if (solution.IsSolved)
        {
            // If solution is found the Total Cost must be less than or equal to budget 
            solution.TotalCostMinorUnits.Should().BeLessThanOrEqualTo(40_000);
        }
        else
        {
            // Checks that the solver does not return partial assignment
            solution.Status.Should().Be(PlanningStatus.Exhausted);
            solution.Assignment.Should().BeEmpty();
        }
    }

    // Checks that the repeat gap is not violated
    [Fact]
    public void Dijkstra_Should_AvoidRepeatingSameDishInAdjacentSlots()
    {
        // Creates o problem with a gap
        PlanningProblem problem = Problem(seed: 8, minDaysBetweenRepeats: 2, maxRepeats: 5);

        PlanSolution solution = new LayeredGraphDijkstraSolver().Solve(problem);

        if (!solution.IsSolved)
        {
            return;
        }

        for (int i = 0; i + 1 < solution.Assignment.Count; i++)
        {
            int dayA = problem.Spec.Slots[i].Day;
            int dayB = problem.Spec.Slots[i + 1].Day;

            // If two consecutively selected slots contain same dish,
            // their day numbers must differ by at least two days
            if (solution.Assignment[i] == solution.Assignment[i + 1])
            {
                Math.Abs(dayB - dayA).Should().BeGreaterThanOrEqualTo(2);
            }
        }
    }

    // Checks that Dijkstra does not violate constraints with 5 seeds
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Dijkstra_Should_NeverViolateConstraints(int seed)
    {
        PlanningProblem problem = Problem(
            dishCount: 150,
            seed: seed,
            budgetMinorUnits: 400_000 + (seed * 30_000),
            maxRepeats: 2 + (seed % 3),
            minDaysBetweenRepeats: seed % 3,
            maxDailyCalories: 1900 + (seed * 120));

        PlanSolution solution = new LayeredGraphDijkstraSolver().Solve(problem);

        if (solution.IsSolved)
        {
            // If the Dijkstra returns a solved plan, no constraints must be violated
            ConstraintValidator.Validate(problem, [.. solution.Assignment])
                .Should().Be(ConstraintViolation.None);
        }
        else
        {
            // Checks that the solver does not return partial
            // assignment and returns the reason of failure
            solution.Assignment.Should().BeEmpty();
            solution.FailureReason.Should().NotBeNullOrWhiteSpace();
        }
    }

    // Finds minimum path objective by exhaustively checking all
    // possible dish assignmnents for a small planning problem
    private static double BruteForceMinimum(PlanningProblem problem)
    {
        int slotCount = problem.SlotCount;
        var domains = new int[slotCount][];

        // Stores candidate domain for every slot
        for (int slot = 0; slot < slotCount; slot++)
        {
            domains[slot] = [.. problem.Domains.ForSlot(problem.Spec.Slots[slot])];
        }

        var current = new int[slotCount];
        double best = double.PositiveInfinity;

        Enumerate(0);

        return best;

        // Recursive function tries every
        // possible dish fron every slot's domain
        void Enumerate(int slot)
        {
            // If all slots are assigned this plan is compared with current best
            if (slot == slotCount)
            {
                best = Math.Min(best, PathObjective.Evaluate(problem, current));

                return;
            }

            foreach (int dish in domains[slot])
            {
                // Skips repeated dishes that violate minimum repeat gap
                if (slot > 0 && current[slot - 1] == dish && problem.Spec.MinDaysBetweenRepeats > 0)
                {
                    int previousDay = problem.Spec.Slots[slot - 1].Day;
                    int currentDay = problem.Spec.Slots[slot].Day;

                    if (Math.Abs(currentDay - previousDay) < problem.Spec.MinDaysBetweenRepeats)
                    {
                        continue;
                    }
                }

                // Assigns the dish and continues with the next slot
                current[slot] = dish;
                Enumerate(slot + 1);
            }
        }
    }
}
