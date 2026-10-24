using System.Runtime.CompilerServices;
using MealPlanner.Planning.DataStructures;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Tests;

// Tests for CandidateDomains
public sealed class CandidateDomainsTests
{
    // Checks that the dish with an excluded allergen is dropped
    [Fact]
    public void Drops_a_dish_with_an_excluded_allergen()
    {
        // Creates a domain based of a planning specification
        // with an excluded allergen at bit position 0
        CandidateDomains domains = Build(PlanningSpec.Create(
            100_000,
            1,
            [MealKinds.Breakfast],
            excludedAllergens: AllergenMask.FromPosition(0)));

        // Checks that Breakfast domain is equal to [0, 2] - dish 1 is dropped
        Assert.Equal([0, 2], domains.ForKind(MealKinds.Breakfast).ToArray());

        // Checks that the domain is not empty
        Assert.False(domains.HasEmptyDomain);
    }

    // Checks that the dish with a disliked ingredient is dropped
    [Fact]
    public void Drops_a_dish_with_a_disliked_ingredient()
    {
        // Creates a domain based of a planning 
        // specification with a disliked ingredient 1
        CandidateDomains domains = Build(PlanningSpec.Create(
            100_000,
            1,
            [MealKinds.Breakfast],
            dislikedIngredients: [1]));

        // Checks that the dish with a disliked ingredient is dropped from domain
        Assert.Equal([1, 2], domains.ForKind(MealKinds.Breakfast).ToArray());

        // Checks that 1 dish was rejected because
        // of ingredient in the statistics (Stats)
        Assert.Equal(1, domains.Stats.RejectedByIngredients);
    }

    // Checks that the dish with a mismatch of diet level is dropped
    [Fact]
    public void Keeps_only_dishes_strict_enough_for_the_diet()
    {
        // Creates a domain based of a planning 
        // specification with a diet level 2 (vegan)
        CandidateDomains domains = Build(PlanningSpec.Create(
            100_000,
            1,
            [MealKinds.Breakfast],
            dietLevel: 2));

        // Checks that only dish with the matching diet level is left
        Assert.Equal([2], domains.ForKind(MealKinds.Breakfast).ToArray());
    }

    // Checks that the empty domain is reported
    [Fact]
    public void Reports_an_empty_domain_when_no_dish_fits_the_meal()
    {
        // Creates a domain based of a planning 
        // specification with only 1 dinner
        CandidateDomains domains = Build(PlanningSpec.Create(
            100_000,
            1,
            [MealKinds.Dinner]));

        // Since testing DishCatalog has no dishes for dinner the
        // candidate domain must be empty and HasEmptyDomain is true
        Assert.Empty(domains.ForKind(MealKinds.Dinner).ToArray());
        Assert.True(domains.HasEmptyDomain);
    }

    // Checks that the pinned slot is not changed
    [Fact]
    public void Pinned_slot_keeps_only_that_dish()
    {
        // Creates a domain based of a planning 
        // specification with pinned slot 0 (dish 0)
        PlanningSpec spec = PlanningSpec.Create(
            100_000,
            1,
            [MealKinds.Breakfast],
            pinnedSlots: new Dictionary<int, int> { [0] = 0 });
        CandidateDomains domains = Build(spec);

        // Checks that domain for slot 0 consists only of pinned dish 0
        Assert.Equal([0], domains.ForSlot(spec.Slots[0]).ToArray());

        // Checks that the domain is not empty
        Assert.False(domains.HasEmptyDomain);
    }

    // Checks that the infeasible pinned slot is emptied
    [Fact]
    public void Infeasible_pin_empties_the_slot()
    {
        // Creates a planning specification for breakfast with pinned
        // slot 0 to dish 3, which can only be eaten for lunch
        PlanningSpec spec = PlanningSpec.Create(
            100_000,
            1,
            [MealKinds.Breakfast],
            pinnedSlots: new Dictionary<int, int> { [0] = 3 });
        CandidateDomains domains = Build(spec);

        // Checks that domains for breakfast slot is empty since
        // the pinned dish does not fit the meal kind
        Assert.Empty(domains.ForSlot(spec.Slots[0]).ToArray());
        Assert.True(domains.HasEmptyDomain);
    }

    // Builds a CandidateDomains based on the test DishCatalog and specification
    private static CandidateDomains Build(PlanningSpec spec)
    {
        DishCatalog catalog = Catalog();

        return CandidateDomains.Build(catalog, DishIndex.Build(catalog), spec);
    }

    // Creates a test DishCatalog
    private static DishCatalog Catalog() => CatalogSamples.Create(
        new CatalogSamples.Draft(100, 300, MealKinds.Breakfast, [1]),
        new CatalogSamples.Draft(120, 350, MealKinds.Breakfast, [2], AllergenMask.FromPosition(0)),
        new CatalogSamples.Draft(80, 250, MealKinds.Breakfast, [3], DietLevel: 2),
        new CatalogSamples.Draft(200, 500, MealKinds.Lunch, [4]));
}
