using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;
using MealPlanner.Planning.Scoring;

namespace MealPlanner.Planning.Models;

// Creates a full planning problem for the solver
public sealed class PlanningProblem
{
    // Private constructor so that the Planning Problem can only be created using the Create method with validation
    private PlanningProblem(
        DishCatalog catalog,
        DishIndex index,
        PlanningSpec spec,
        CandidateDomains domains,
        ObjectiveWeights weights,
        SimilarityMetric similarity,
        int seed)
    {
        Catalog = catalog;
        Index = index;
        Spec = spec;
        Domains = domains;
        Weights = weights;
        Similarity = similarity;
        Seed = seed;
    }

    public DishCatalog Catalog { get; }

    public DishIndex Index { get; }

    public PlanningSpec Spec { get; }

    // Domains containing dishes after filters
    public CandidateDomains Domains { get; }

    public ObjectiveWeights Weights { get; }

    public SimilarityMetric Similarity { get; }

    // Stores the seed for random operations.
    // For reproducability with the same input and seed.
    public int Seed { get; }

    // The number of slots in the meal plan
    public int SlotCount => Spec.SlotCount;

    // Creates a new Planning Problem after validation
    public static PlanningProblem Create(
        DishCatalog catalog,
        PlanningSpec spec,
        ObjectiveWeights? weights = null,
        int seed = 0,
        DishIndex? index = null,
        SimilarityMetric? similarity = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(spec);

        // If an existing index is supplied makes sure that the correct catalog was supplied
        // Wrong solutions can be made with the wrong catalog used
        if (index is not null && index.IngredientCount != catalog.IngredientCount)
        {
            throw new ArgumentException(
                "The index was built from another catalog.",
                nameof(index));
        }

        if (similarity is not null && !ReferenceEquals(similarity.Catalog, catalog))
        {
            throw new ArgumentException(
                "The similarity metric was built from another catalog.",
                nameof(similarity));
        }

        // If index is not null use it, otherwise build a new DishIndex from catalog
        DishIndex dishIndex = index ?? DishIndex.Build(catalog);

        // Builds the CandidateDomains - possible dishes
        CandidateDomains domains = CandidateDomains.Build(catalog, dishIndex, spec);

        return new PlanningProblem(
            catalog,
            dishIndex,
            spec,
            domains,
            weights ?? ObjectiveWeights.Default,
            // Similarity cash resides within SimilarityMetric instance. If it doesn't
            // exist it's created here once per catalog. Cannot be build by request.
            similarity ?? new SimilarityMetric(catalog),
            seed);
    }

    // Penalty for not following users cuisine preference. 0 if no
    // preferences or the dish of preferred cuisine was chosen, 1 if not.
    public double PreferencePenalty(int dish)
    {
        if (Spec.PreferredCuisines.Count == 0)
        {
            return 0.0;
        }

        return Spec.PreferredCuisines.Contains(Catalog.CuisineIds[dish]) ? 0.0 : 1.0;
    }
}
