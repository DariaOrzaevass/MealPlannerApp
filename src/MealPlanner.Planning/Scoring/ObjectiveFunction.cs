using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Scoring;

// Calculates an objective function for a complete meal plan.
// Gives a way to compare them and algorithms used to make them
public static class ObjectiveFunction
{
    // Evaluates an objective function for the finished assignment of dishes to slots
    public static double Evaluate(PlanningProblem problem, ReadOnlySpan<int> assignment)
    {
        // Firstly checks that the problem and the assignment are not null
        ArgumentNullException.ThrowIfNull(problem);

        if (assignment.IsEmpty)
        {
            return double.PositiveInfinity;
        }

        DishCatalog catalog = problem.Catalog;
        ObjectiveWeights weights = problem.Weights;

        long totalCost = 0;
        double preferencePenalty = 0;

        // Goes through all assigned dishes summing
        // total cost and total preference penalty of the plan
        foreach (int dish in assignment)
        {
            totalCost += catalog.CostMinorUnits[dish];
            preferencePenalty += problem.PreferencePenalty(dish);
        }

        // Fraction of the budget spend
        double costTerm = (double)totalCost / problem.Spec.BudgetMinorUnits;

        // Average similarity of dishes standing next to each other in the plan
        double monotonyTerm = 0;

        for (int i = 0; i + 1 < assignment.Length; i++)
        {
            monotonyTerm += problem.Similarity.Between(assignment[i], assignment[i + 1]);
        }

        if (assignment.Length > 1)
        {
            monotonyTerm /= assignment.Length - 1;
        }

        // Average preference penalty for all dishes
        double preferenceTerm = preferencePenalty / assignment.Length;

        // Variety in the plan - how many repetitions are there
        double varietyTerm = VarietyTerm(assignment);

        // returns all terms multiplied by their weights
        return (weights.Cost * costTerm)
            + (weights.Monotony * monotonyTerm)
            + (weights.Preference * preferenceTerm)
            + (weights.Variety * varietyTerm);
    }

    // Same as Evaluate, but without the variety penalty,
    // if diversity needs to be handled separately
    public static double EvaluateWithoutVariety(
       PlanningProblem problem,
       ReadOnlySpan<int> assignment)
    {
        ArgumentNullException.ThrowIfNull(problem);

        if (assignment.IsEmpty)
        {
            return double.PositiveInfinity;
        }

        ObjectiveWeights weights = problem.Weights;

        return Evaluate(problem, assignment)
            - (weights.Variety * VarietyTerm(assignment));
    }

    // The lack of variety from 0 to 1
    private static double VarietyTerm(ReadOnlySpan<int> assignment) =>
    1.0 - ((double)CountDistinct(assignment) / assignment.Length);

    // How many different dishes are in the assignment.
    // Does not allocate memory like other methods could
    internal static int CountDistinct(ReadOnlySpan<int> assignment)
    {
        // Count of the distinct dishes in the assignment
        int distinct = 0;

        for (int i = 0; i < assignment.Length; i++)
        {
            bool seenEarlier = false;

            // Checks the current dish with previous
            for (int j = 0; j < i; j++)
            {
                if (assignment[j] == assignment[i])
                {
                    seenEarlier = true;

                    break;
                }
            }

            if (!seenEarlier)
            {
                distinct++;
            }
        }

        return distinct;
    }
}
