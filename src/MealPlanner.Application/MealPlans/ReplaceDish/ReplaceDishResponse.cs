using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Application.MealPlans.ReplaceDish;

// What dish was replaced and to what dish
public sealed record ReplaceDishResponse(
    Guid OldDishId,
    string OldDishName,
    Guid NewDishId,
    string NewDishName,
    decimal NewCost,
    decimal TotalCost,
    double Distance,
    int PathLength,
    long ExploredNodes);
