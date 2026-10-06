using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Domain.MealPlans;

// Record storing all contraints of the meal plan
public sealed record PlanningConstraints
{
    // Maximum duration of the meal plan
    public const int MaxDurationDays = 28;

    // Default constructor for EF core to get the constraints of the meal plan from the database
    private PlanningConstraints()
    {
    }

    // Private constructor so that the PlanningConstraints can only be created using the Create method with validation
    private PlanningConstraints(
    Money budget,
    int durationDays,
    MealType[] mealsPerDay,
    DietType diet,
    AllergenId[] excludedAllergens,
    IngredientId[] dislikedIngredients,
    int maxRepeatsPerPlan,
    int minDaysBetweenRepeats,
    int? minDailyCalories,
    int? maxDailyCalories,
    int? maxCookingTimeMinutes,
    Cuisine[] preferredCuisines)
    {
        Budget = budget;
        DurationDays = durationDays;
        MealsPerDay = mealsPerDay;
        Diet = diet;
        ExcludedAllergens = excludedAllergens;
        DislikedIngredients = dislikedIngredients;
        MaxRepeatsPerPlan = maxRepeatsPerPlan;
        MinDaysBetweenRepeats = minDaysBetweenRepeats;
        MinDailyCalories = minDailyCalories;
        MaxDailyCalories = maxDailyCalories;
        MaxCookingTimeMinutes = maxCookingTimeMinutes;
        PreferredCuisines = preferredCuisines;
    }

    // The budget constraint
    public Money Budget { get; init; } = null!;

    // The duration of the meal plan in days
    public int DurationDays { get; init; }

    // Can be only Breakfast and Dinner for exaple, could be useful for diets
    public IReadOnlyList<MealType> MealsPerDay { get; init; } = [];

    // Type of the diet Omnivore/Vegetarian/Vegan
    public DietType Diet { get; init; }

    // Which allergies the user has
    public IReadOnlyList<AllergenId> ExcludedAllergens { get; init; } = [];

    // Which ingredient the user dislike
    public IReadOnlyList<IngredientId> DislikedIngredients { get; init; } = [];

    // The maximum amount of times the dish can repeat in plan
    public int MaxRepeatsPerPlan { get; init; }

    // The minimum days between repeats
    public int MinDaysBetweenRepeats { get; init; }

    // The minimum amount of calories per day
    public int? MinDailyCalories { get; init; }

    // The maximum amount of calories per day
    public int? MaxDailyCalories { get; init; }

    // The maximum cooking time
    public int? MaxCookingTimeMinutes { get; init; }

    // Preferred cuisines of the user
    public IReadOnlyList<Cuisine> PreferredCuisines { get; init; } = [];

    // The total amount of meals per plan
    public int SlotCount => DurationDays * MealsPerDay.Count;

    // Creates PlanningConstraints of the user after validation of the input
    public static Result<PlanningConstraints> Create(
    Money budget,
    int durationDays,
    IReadOnlyList<MealType> mealsPerDay,
    DietType diet,
    IReadOnlyList<AllergenId> excludedAllergens,
    IReadOnlyList<IngredientId> dislikedIngredients,
    int maxRepeatsPerPlan,
    int minDaysBetweenRepeats,
    int? minDailyCalories = null,
    int? maxDailyCalories = null,
    int? maxCookingTimeMinutes = null,
    IReadOnlyList<Cuisine>? preferredCuisines = null)
    {
        if (budget.Amount <= 0)
        {
            return Result.Failure<PlanningConstraints>(MealPlanErrors.NonPositiveBudget);
        }

        if (durationDays is < 1 or > MaxDurationDays)
        {
            return Result.Failure<PlanningConstraints>(MealPlanErrors.DurationOutOfRange);
        }

        if (mealsPerDay.Count == 0)
        {
            return Result.Failure<PlanningConstraints>(MealPlanErrors.NoMealsSelected);
        }

        return new PlanningConstraints(
            budget,
            durationDays,
            [.. mealsPerDay],
            diet,
            [.. excludedAllergens.Distinct()],
            [.. dislikedIngredients.Distinct()],
            maxRepeatsPerPlan,
            minDaysBetweenRepeats,
            minDailyCalories,
            maxDailyCalories,
            maxCookingTimeMinutes,
            preferredCuisines is null ? [] : [.. preferredCuisines.Distinct()]);
    }
}
