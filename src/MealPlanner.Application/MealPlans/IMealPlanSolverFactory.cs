using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Planning.Solvers;

namespace MealPlanner.Application.MealPlans;

// Provides a solver by its domain-level algorithm designation.
// Application layer does not create algorithms.
public interface IMealPlanSolverFactory
{
    Result<IMealPlanSolver> Create(PlannerAlgorithm algorithm);

    // Which algorithms are available. For UI
    IReadOnlyCollection<PlannerAlgorithm> Available { get; }
}
