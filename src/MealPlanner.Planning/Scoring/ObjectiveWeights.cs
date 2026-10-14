using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Scoring;

// Objective weights: Cost - weight of the spent budget,
// Monotony - weight of similar dishes being close,
// Preference - weight of not following the users preferences
// Variety - weight of lack of the variety of dishes
public sealed record ObjectiveWeights(
    double Cost,
    double Monotony,
    double Preference,
    double Variety)
{
    // Default weights
    public static readonly ObjectiveWeights Default = new(1.0, 0.6, 0.4, 0.8);

    // Only value cost do not care about monotony, preference, variety. For checks.
    public static readonly ObjectiveWeights CostOnly = new(1.0, 0.0, 0.0, 0.0);
}
