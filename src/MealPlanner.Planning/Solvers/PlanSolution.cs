using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace MealPlanner.Planning.Solvers;

// Result of the solver algorithm
public sealed class PlanSolution
{
    private PlanSolution(
    PlanningStatus status)
    {
        Status = status;
    }

    public PlanningStatus Status { get; }

    // Returns whether the plan was solved or not
    public bool IsSolved => Status == PlanningStatus.Solved;
}
