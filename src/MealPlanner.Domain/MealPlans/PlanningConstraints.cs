using System;
using System.Collections.Generic;
using System.Text;
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

    private PlanningConstraints(
    Money budget,
    int durationDays,
    MealType[] mealsPerDay,
    DietType diet,
    AllergenId[] excludedAllergens,
    IngredientId[] dislikedIngredients,
    int maxRepeatsPerPlan)
    {
        Budget = budget;
        DurationDays = durationDays;
        MealsPerDay = mealsPerDay;
        Diet = diet;
        ExcludedAllergens = excludedAllergens;
        DislikedIngredients = dislikedIngredients;
        MaxRepeatsPerPlan = maxRepeatsPerPlan;
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
}
