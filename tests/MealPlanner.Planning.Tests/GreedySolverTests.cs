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

        // Checks that algorithm successfully created a meal plen
        solution.IsSolved.Should().BeTrue(solution.FailureReason);

        // Checks that all 21 slots are filled
        solution.Assignment.Should().HaveCount(21);

        // Checks that all assigned dishes has valid Ids (0 to 199)
        solution.Assignment.Should().OnlyContain(d => d >= 0 && d < problem.Catalog.Count);
    }
}
