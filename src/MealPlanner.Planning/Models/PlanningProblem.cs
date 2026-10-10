using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Models;

// Creates a full planning problem for the solver
public sealed class PlanningProblem
{
    // Private constructor so that the Planning Problem can only be created using the Create method with validation
    private PlanningProblem()
    {

    }

    // Creates a new Planning Problem after validation
    public static PlanningProblem Create()
    {
        return new PlanningProblem();
    }
}
