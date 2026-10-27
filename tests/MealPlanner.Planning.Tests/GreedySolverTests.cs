using FluentAssertions;
using MealPlanner.Planning.DataStructures;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Solvers;
using MealPlanner.Planning.Tests.Models;

namespace MealPlanner.Planning.Tests;

// Tests for GreedySolver
public class GreedySolverTests
{
    // Most tests use this standart schedule
    private static readonly MealKinds[] _threeMeals =
        [MealKinds.Breakfast, MealKinds.Lunch, MealKinds.Dinner];

    // Creates a GreedySolver instance shared by all tests
    private static readonly GreedySolver _solver = new();

    // Creates a complete testing PlanningProblem
    private static PlanningProblem Problem(
    int dishCount = 120,
    int seed = 1,
    int budgetMinorUnits = 500_000,
    int durationDays = 7,
    int dietLevel = 0,
    AllergenMask excluded = default,
    IReadOnlyList<int>? disliked = null,
    int maxRepeats = 3,
    int minDaysBetweenRepeats = 2,
    int maxDailyCalories = int.MaxValue,
    int maxCookingTimeMinutes = int.MaxValue) =>
    PlanningProblem.Create(
        RandomCatalog.Build(dishCount, seed),
        PlanningSpec.Create(
            budgetMinorUnits,
            durationDays,
            _threeMeals,
            dietLevel: dietLevel,
            excludedAllergens: excluded,
            dislikedIngredients: disliked,
            maxRepeats: maxRepeats,
            minDaysBetweenRepeats: minDaysBetweenRepeats,
            maxDailyCalories: maxDailyCalories,
            maxCookingTimeMinutes: maxCookingTimeMinutes),
        seed: seed);

    // If greedy algorithm solved the problem, all slots should be filed
    [Fact]
    public void Solve_Should_FillEverySlot()
    {
        // Creates a default problem for 7 days and 3 meals gives 21 slots
        PlanningProblem problem = Problem();

        // Runs the Greedy Solver
        PlanSolution solution = _solver.Solve(problem);

        // Checks that algorithm successfully created a meal plan
        solution.IsSolved.Should().BeTrue(solution.FailureReason);

        // Checks that all 21 slots are filled
        solution.Assignment.Should().HaveCount(21);

        // Checks that all assigned dishes has valid Ids (0 to 199)
        solution.Assignment.Should().OnlyContain(d => d >= 0 && d < problem.Catalog.Count);
    }

    // If greedy algorithm solved the problem, no constraints should be violated
    [Fact]
    public void Solve_Should_ProduceFeasiblePlan()
    {
        PlanningProblem problem = Problem();

        PlanSolution solution = _solver.Solve(problem);

        // Checks that no constraints were violated if greedy successfully created a plan
        ConstraintValidator.Validate(problem, [.. solution.Assignment])
            .Should().Be(ConstraintViolation.None);
    }

    // If greedy algorithm solved the problem, budget should not violated
    [Fact]
    public void Solve_Should_RespectBudget()
    {
        // Creates a stricter budget
        PlanningProblem problem = Problem(budgetMinorUnits: 400_000);

        PlanSolution solution = _solver.Solve(problem);

        // Checks that algorithm created a meal plan and budget is not violated
        solution.IsSolved.Should().BeTrue(solution.FailureReason);
        solution.TotalCostMinorUnits.Should().BeLessThanOrEqualTo(400_000);
    }

    // Checks that GreedySolver correctly reports the total cost of final assignment
    [Fact]
    public void Solve_Should_ReportTotalCostMatchingAssignment()
    {
        PlanningProblem problem = Problem();

        PlanSolution solution = _solver.Solve(problem);

        // Recalculates the total cost of final assignment independently of the solver
        int recomputed = solution.Assignment.Sum(d => problem.Catalog.CostMinorUnits[d]);
        solution.TotalCostMinorUnits.Should().Be(recomputed);
    }

    // Checks that GreedySolver provides same results for same planning problem
    [Fact]
    public void Solve_Should_BeDeterministic()
    {
        // Creates two plan solutions with the same seed
        PlanSolution first = _solver.Solve(Problem(seed: 42));
        PlanSolution second = _solver.Solve(Problem(seed: 42));

        // Checks that these solutiona are the same
        first.Assignment.Should().Equal(second.Assignment);
        first.TotalCostMinorUnits.Should().Be(second.TotalCostMinorUnits);
        first.Objective.Should().Be(second.Objective);
    }

    // If at least one meal slot has no candidate dishes the status will be Infeasible
    [Fact]
    public void Solve_WithEmptyDomain_Should_ReportInfeasible()
    {
        // Vegan user with all possible allergies
        PlanningProblem problem = Problem(
            dietLevel: 2,
            excluded: AllergenMask.FromPositions([0, 1, 2, 3, 4, 5, 6, 7, 8]),
            maxCookingTimeMinutes: 6);

        PlanSolution solution = _solver.Solve(problem);

        // It is impossible to find solutions, so the status is infeasible
        solution.Status.Should().Be(PlanningStatus.Infeasible);

        // Solver should report failure with the reason of it
        solution.FailureReason.Should().NotBeNullOrWhiteSpace();
    }

    // GreedySolver can fail, but cannot provide the user with invalid plan
    [Fact]
    public void Solve_WithImpossibleBudget_Should_NotReturnInvalidPlan()
    {
        // The budget is intentionally too small
        PlanningProblem problem = Problem(budgetMinorUnits: 30_000);

        PlanSolution solution = _solver.Solve(problem);

        // Solver should not return invalid plan,
        // it should return the failure with its reason
        solution.IsSolved.Should().BeFalse();
        solution.Assignment.Should().BeEmpty();
        solution.FailureReason.Should().NotBeNullOrWhiteSpace();
    }

    // Allergens can be dangerous for user's health, so they demand the separate test 
    [Fact]
    public void Solve_Should_NeverPlaceDishWithExcludedAllergen()
    {
        // AllergenMask containing allergen positions 3 and 5 corresponding to nuts
        AllergenMask nuts = AllergenMask.FromPositions([3, 5]);

        // Creates a problem that demands nuts to be excluded from the plan
        PlanningProblem problem = Problem(excluded: nuts);

        PlanSolution solution = _solver.Solve(problem);

        // if the plan is solved sucessfully it should not have dishes with nuts
        solution.IsSolved.Should().BeTrue(solution.FailureReason);

        // Checks every dish individually, none should have any allergens - no intersection
        foreach (int dish in solution.Assignment)
        {
            problem.Catalog.Allergens[dish].Intersects(nuts).Should().BeFalse();
        }
    }

    // Checks that solved plan does not violate max repeats of dish
    [Fact]
    public void Solve_Should_RespectMaxRepeats()
    {
        // Problem states that a dish cannot repeat for more than 2 times
        PlanningProblem problem = Problem(maxRepeats: 2, minDaysBetweenRepeats: 1);

        PlanSolution solution = _solver.Solve(problem);

        solution.IsSolved.Should().BeTrue(solution.FailureReason);

        // Groups all occurences by dish, where each group has no more than 2 dishes
        solution.Assignment.GroupBy(d => d).Should().OnlyContain(g => g.Count() <= 2);
    }
}
