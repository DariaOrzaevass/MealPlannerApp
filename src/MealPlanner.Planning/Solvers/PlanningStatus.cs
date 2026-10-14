using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Solvers;

// How the work of the solver ended
public enum PlanningStatus
{
    // The plan was successfully constructed and fits all hard constraints
    Solved = 0,

    // There is no solution - some constraint(s) make it impossible
    Infeasible = 1,

    // The solver stopped because it run out of time or reached the
    // limit of nodes and could not find the solution or its absence.
    Exhausted = 2
}
