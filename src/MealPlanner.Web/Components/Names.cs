using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Web.Components;

// Human readible names for enums. Not in the Domain layer, because
// values like "LaueredGraphDijkstra" should not be shown to the
// user directly and translating them is the responsibility of the UI.
internal static class Names
{
    // This switch handels all named enum members explicitly. When a new algorithm is added,
    // the project will not compile without adding the algorithm here too. CS8524 is disabled
    // separately, because it concerns unnamed integer enum values, not a forgotten named member.
    internal static string Of(PlannerAlgorithm algorithm)
    {
#pragma warning disable CS8524
        return algorithm switch
        {
            PlannerAlgorithm.Greedy => "Greedy",
            PlannerAlgorithm.LayeredGraphDijkstra => "Dijkstra (Layered Graph)",
            PlannerAlgorithm.Backtracking => "Backtracking",
            PlannerAlgorithm.BranchAndBound => "Branch and Bound"
        };
#pragma warning restore CS8524
    }

    internal static string Of(MealType meal) => meal switch
    {
        MealType.Breakfast => "Breakfast",
        MealType.Lunch => "Lunch",
        MealType.Dinner => "Dinner",
        MealType.Snack => "Snack",
        _ => meal.ToString()
    };

    internal static string Of(DietType diet) => diet switch
    {
        DietType.Omnivore => "No restrictions",
        DietType.Vegetarian => "Vegetarian",
        DietType.Vegan => "Vegan",
        _ => diet.ToString()
    };

    internal static string Of(Cuisine cuisine) => cuisine switch
    {
        Cuisine.Other => "Other",
        Cuisine.Russian => "Russian",
        Cuisine.Italian => "Italian",
        Cuisine.Asian => "Asian",
        Cuisine.Mediterranean => "Mediterranean",
        Cuisine.American => "American",
        Cuisine.MiddleEastern => "Middle Eastern",
        _ => cuisine.ToString()
    };
}
