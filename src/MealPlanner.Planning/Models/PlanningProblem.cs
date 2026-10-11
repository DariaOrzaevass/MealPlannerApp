using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;

namespace MealPlanner.Planning.Models;

// Creates a full planning problem for the solver
public sealed class PlanningProblem
{
    // Private constructor so that the Planning Problem can only be created using the Create method with validation
    private PlanningProblem(
        DishCatalog catalog,
        DishIndex index,
        PlanningSpec spec)
    {
        Catalog = catalog;
        Index = index;
        Spec = spec;
    }

    public DishCatalog Catalog { get; }

    public DishIndex Index { get; }

    public PlanningSpec Spec { get; }

    // Creates a new Planning Problem after validation
    public static PlanningProblem Create(
        DishCatalog catalog,
        PlanningSpec spec,
        DishIndex? index = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(spec);

        // If index is not null use it, otherwise build a new DishIndex from catalog
        DishIndex dishIndex = index ?? DishIndex.Build(catalog);

        return new PlanningProblem(
            catalog,
            dishIndex,
            spec);
    }
}
