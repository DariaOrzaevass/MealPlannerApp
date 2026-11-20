using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Web.Contracts;

// A separate request body for meal-plan generation instead of accepting
// command directly, because command have UserId, which the client isn't
// allowed to specify. If the endpoint accepted command directly,
// a client would be able to generate a plan on another user behalf.
public sealed record GenerateMealPlanRequest(
    string Name,
    decimal Budget,
    int DurationDays,
    DietType Diet,
    int MaxRepeatsPerPlan,
    int MinDaysBetweenRepeats,
    PlannerAlgorithm Algorithm,
    DateOnly? StartDate = null,
    string? CurrencyCode = null,
    MealType[]? MealsPerDay = null,
    Guid[]? ExcludedAllergens = null,
    Guid[]? DislikedIngredients = null,
    int? MinDailyCalories = null,
    int? MaxDailyCalories = null,
    int? MaxCookingTimeMinutes = null,
    Cuisine[]? PreferredCuisines = null);
