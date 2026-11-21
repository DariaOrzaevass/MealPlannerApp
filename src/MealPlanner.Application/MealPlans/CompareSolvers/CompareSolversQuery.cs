using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;

namespace MealPlanner.Application.MealPlans.CompareSolvers;

// Runs all solver on the same problem and returns their metrics. It's a query and not a
// command because nothing is saved and 4 unnecessary plans are not added to the database.
public sealed record CompareSolversQuery(PlanningConstraintsInput Constraints)
    : IQuery<IReadOnlyCollection<SolverComparisonRow>>;
