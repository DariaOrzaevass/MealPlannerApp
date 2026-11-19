using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Solvers;

// Result of the substitution search. Distance is the distance to the substitution
// in the similarity graph. 0 - indistinguishable, larger - greater distance.
// Path shows the path from rejected dish to the substitution.
// Path longer than 2 - there wasn't any direct edge and
// substitution was reached through the chain of similar dishes.
public sealed record SubstitutionResult(
    bool Found,
    int Dish,
    double Distance,
    IReadOnlyList<int> Path,
    long ExploredNodes,
    string? FailureReason)
{
    public static SubstitutionResult NotFound(string reason, long exploredNodes) =>
        new(Found: false, Dish: -1, double.PositiveInfinity, [], exploredNodes, reason);
}
