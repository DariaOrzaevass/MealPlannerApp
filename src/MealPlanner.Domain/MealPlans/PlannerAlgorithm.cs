using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.MealPlans;

// Algorithms that can be used for meal planning
public enum PlannerAlgorithm
{
    Greedy = 1,

    LayeredGraphDijkstra = 2,

    Backtracking = 3,

    BranchAndBound = 4
}
