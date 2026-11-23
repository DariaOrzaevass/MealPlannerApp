using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Scoring;
using MealPlanner.Planning.Solvers;
using MealPlanner.Planning.Tests.Models;

namespace MealPlanner.Planning.Tests;

// Tests for Branch And Bound
public class BranchAndBoundTests
{
    // Most tests use this standard schedule
    private static readonly MealKinds[] _threeMeals =
        [MealKinds.Breakfast, MealKinds.Lunch, MealKinds.Dinner];

    // Creates a complete testing PlanningProblem
    private static PlanningProblem Problem(
        int dishCount = 120,
        int seed = 1,
        int budgetMinorUnits = 500_000,
        int durationDays = 7,
        IReadOnlyList<MealKinds>? meals = null) =>
        PlanningProblem.Create(
            RandomCatalog.Build(dishCount, seed),
            PlanningSpec.Create(
                budgetMinorUnits,
                durationDays,
                meals ?? _threeMeals,
                maxRepeats: 3,
                minDaysBetweenRepeats: 2),
            seed: seed);

    // Creates a solver in Branch and Bound mode
    private static BacktrackingSolver Optimizing(TimeSpan? limit = null) =>
        new(timeLimit: limit ?? TimeSpan.FromMilliseconds(500), optimize: true);

    // Checks that name of BacktrackingSolver reflects the selected mode
    [Fact]
    public void Name_Should_ReflectMode()
    {
        new BacktrackingSolver(optimize: true).Name.Should().Be("Branch and Bound");
        new BacktrackingSolver().Name.Should().Be("Backtracking");
    }

    // Checks that the optimizing mode never returns a
    // solution that is worse than one found by Backtracking
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Optimizing_Should_NotBeWorseThanSatisfying(int seed)
    {
        PlanningProblem problem = Problem(seed: seed);

        //Solves the problem with Backtracking, then with Branch and Bound 
        PlanSolution firstFeasible = new BacktrackingSolver().Solve(problem);
        PlanSolution best = Optimizing().Solve(problem);

        firstFeasible.IsSolved.Should().BeTrue(firstFeasible.FailureReason);
        best.IsSolved.Should().BeTrue(best.FailureReason);

        // Checks that the solution with optimization does not have a higher objective
        best.Objective.Should().BeLessThanOrEqualTo(firstFeasible.Objective + 1e-9);
    }

    // If the algorithm solved the problem, no constraints should be violated
    [Fact]
    public void Optimizing_Should_ProduceFeasiblePlan()
    {
        // Creates a default problem. For 7 days and 3 meals that gives 21 
        PlanningProblem problem = Problem(seed: 5);

        // Runs the Backtracking in optimixing mode
        PlanSolution solution = Optimizing().Solve(problem);

        // Checks that the algorithm successfully created a meal plan
        solution.IsSolved.Should().BeTrue(solution.FailureReason);

        // Checks that no constraints were violated
        ConstraintValidator.Validate(problem, [.. solution.Assignment])
            .Should().Be(ConstraintViolation.None);
    }

    // Checks that Branch and Bound finds the true minimum of
    // layered-graph objective by comparing it to Brute Force solution
    [Fact]
    public void Optimizing_Should_MatchBruteForce_OnTinyProblem()
    {
        // Creates a small problem
        PlanningProblem problem = PlanningProblem.Create(
            RandomCatalog.Build(dishCount: 12, seed: 77, ingredientPool: 8),
            PlanningSpec.Create(
                budgetMinorUnits: 200_000,
                durationDays: 3,
                mealsPerDay: [MealKinds.Lunch, MealKinds.Dinner],
                maxRepeats: 3,
                minDaysBetweenRepeats: 1),
            seed: 77);

        // Solves the problem with Branch and Bound
        PlanSolution solution = new BacktrackingSolver(
            timeLimit: TimeSpan.FromSeconds(5), optimize: true).Solve(problem);

        solution.IsSolved.Should().BeTrue(solution.FailureReason);
        solution.IsProvenOptimal.Should().BeTrue("the search tree for this problem should be fully exhausted");

        // Brute Force reference solution
        double bruteForceBest = BruteForceOptimum(problem);

        // Checks that Branch and Bound gets the same minimum objective as exhaustive search
        solution.Objective.Should().BeApproximately(bruteForceBest, 1e-9);
    }

    // Checks that none of the solvers of the application prodices Path
    // Objective value that is lower than the Dijkstra lower bound.
    [Fact]
    public void Solvers_Should_NotFallBelowDijkstraLowerBound_OnTinyProblem()
    {
        // Creates a small problem
        PlanningProblem problem = PlanningProblem.Create(
            RandomCatalog.Build(dishCount: 12, seed: 77, ingredientPool: 8),
            PlanningSpec.Create(
                budgetMinorUnits: 200_000,
                durationDays: 3,
                mealsPerDay: [MealKinds.Lunch, MealKinds.Dinner],
                maxRepeats: 3,
                minDaysBetweenRepeats: 1),
            seed: 77);

        // Calculates the shortest-path lower bound
        var dijkstraSolver = new LayeredGraphDijkstraSolver();
        double bound = dijkstraSolver.LowerBound(problem);

        // Runs solvers
        PlanSolution dijkstra = dijkstraSolver.Solve(problem);
        PlanSolution greedy = new GreedySolver().Solve(problem);
        PlanSolution branchAndBound = new BacktrackingSolver(
            timeLimit: TimeSpan.FromSeconds(5), optimize: true).Solve(problem);

        // if solvers found a solution
        dijkstra.IsSolved.Should().BeTrue(dijkstra.FailureReason);
        greedy.IsSolved.Should().BeTrue(greedy.FailureReason);
        branchAndBound.IsSolved.Should().BeTrue(branchAndBound.FailureReason);

        // Verifies that Dijkstra's solution has essentially the same objective as lower bound
        PathObjective.Evaluate(problem, dijkstra.Assignment).Should().BeApproximately(bound, 1e-9);

        // Checks that neither Greedy nor Branch and Bound obtain the objective that is better than the lower bound
        PathObjective.Evaluate(problem, greedy.Assignment).Should().BeGreaterThanOrEqualTo(bound - 1e-9);
        PathObjective.Evaluate(problem, branchAndBound.Assignment).Should().BeGreaterThanOrEqualTo(bound - 1e-9);
    }

    // Checks that optimistic lower-bound estimate of the best possible
    // objective for every partial solution is never bigger than the
    // objective of any possible completion of this solution.
    [Fact]
    public void LowerBound_Should_NeverExceedObjectiveOfAnyCompletion()
    {
        PlanningProblem problem = PlanningProblem.Create(
            RandomCatalog.Build(dishCount: 9, seed: 91, ingredientPool: 6),
            PlanningSpec.Create(
                budgetMinorUnits: 400_000,
                durationDays: 2,
                mealsPerDay: [MealKinds.Lunch, MealKinds.Dinner]),
            seed: 91);

        int slotCount = problem.SlotCount;
        var domains = new int[slotCount][];

        // stores the candidate domain for each slot
        for (int slot = 0; slot < slotCount; slot++)
        {
            domains[slot] = [.. problem.Domains.ForSlot(problem.Spec.Slots[slot])];
        }

        // Creates same search context that is used by Branch and Bound.
        // Heuristics are disabled so lower-bound logic is directly tested
        var context = new SearchContext(
            problem,
            BacktrackingHeuristics.None,
            TimeSpan.FromSeconds(30),
            nodeLimit: long.MaxValue,
            startTicks: 0,
            optimize: true,
            CancellationToken.None);

        int checkedPrefixes = 0;

        WalkPrefixes(0);

        checkedPrefixes.Should().BeGreaterThan(0, "at least one prefix should have been checked");

        void WalkPrefixes(int depth)
        {
            // Calculates lower bound for the current partial assignment
            double bound = context.LowerBoundObjective();
            checkedPrefixes++;

            var completion = new int[slotCount];

            // copies already possible way of completing the current prefix
            for (int i = 0; i < depth; i++)
            {
                completion[i] = context.State.Assignment[i];
            }

            CheckCompletions(depth);

            if (depth == slotCount)
            {
                return;
            }

            // Extends the current prefix with every possible dish in this slot
            foreach (int dish in domains[depth])
            {
                SlotDescriptor slot = problem.Spec.Slots[depth];
                context.State.Place(slot, dish);
                context.MarkAssigned(depth);

                WalkPrefixes(depth + 1);

                // Need to restore previous state before trying the next dish
                context.UnmarkAssigned(depth);
                context.State.Remove(slot);
            }

            void CheckCompletions(int from)
            {
                if (from == slotCount)
                {
                    double objective = ObjectiveFunction.Evaluate(problem, completion);

                    objective.Should().BeGreaterThanOrEqualTo(
                        bound - 1e-9,
                        $"lower bound {bound:0.000000} for a prefix of length {depth} "
                        + $"exceeded the completion objective {objective:0.000000}");

                    return;
                }

                foreach (int dish in domains[from])
                {
                    completion[from] = dish;
                    CheckCompletions(from + 1);
                }
            }
        }
    }

    // Checks that Branch and Bound reports that plan is proven optimal only when the tree is exhausted
    [Fact]
    public void Optimizing_Should_ReportProvenOptimalOnlyWhenTreeExhausted()
    {
        // Big problem
        PlanningProblem problem = Problem(dishCount: 400, seed: 8);

        // Short period of time for the works
        PlanSolution solution = Optimizing(TimeSpan.FromMilliseconds(50)).Solve(problem);

        if (solution.IsSolved)
        {
            // It is impossibe to exhaustively search such big tree in this amount of time
            solution.IsProvenOptimal.Should().BeFalse();
        }
    }

    // Checks that Branch and Bound respects the time limit
    [Fact]
    public void Optimizing_Should_RespectTimeLimit()
    {
        PlanningProblem problem = Problem(dishCount: 400, seed: 9);

        PlanSolution solution = Optimizing(TimeSpan.FromMilliseconds(200)).Solve(problem);

        solution.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    // Checks that optimized BacktrackingSolver provides same results for same planning problem
    [Fact]
    public void Optimizing_Should_BeDeterministic()
    {
        // Creates and solves the same problem (with the same seed) twice
        PlanSolution first = Optimizing().Solve(Problem(seed: 17));
        PlanSolution second = Optimizing().Solve(Problem(seed: 17));

        // Checks that the two objective values are identical
        first.Objective.Should().Be(second.Objective);
    }

    // Checks that Branch and Bound returns an objective that is equal to Greedy or better
    [Fact]
    public void Optimizing_Should_BeatGreedy_OnObjective()
    {
        PlanningProblem problem = Problem(seed: 21);

        // Solves the same problem with Greedy and Branch and Bound
        PlanSolution greedy = new GreedySolver().Solve(problem);
        PlanSolution exact = new BacktrackingSolver(
            timeLimit: TimeSpan.FromSeconds(1), optimize: true).Solve(problem);

        greedy.IsSolved.Should().BeTrue(greedy.FailureReason);
        exact.IsSolved.Should().BeTrue(exact.FailureReason);

        // Checks that Branch and Bound's objective is no worse than Greedy's result
        exact.Objective.Should().BeLessThanOrEqualTo(greedy.Objective + 1e-9);
    }

    // Finds optimal objective by exhaustively checking all
    // possible dish assignmnents for a small planning problem
    private static double BruteForceOptimum(PlanningProblem problem)
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
            if (slot == slotCount)
            {
                // Only complete assignments that are feasible are evaluated
                if (ConstraintValidator.IsFeasible(problem, current))
                {
                    // Keeps the best objective found
                    best = Math.Min(best, ObjectiveFunction.Evaluate(problem, current));
                }

                return;
            }

            foreach (int dish in domains[slot])
            {
                current[slot] = dish;
                Enumerate(slot + 1);
            }
        }
    }
}
