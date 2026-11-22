using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Application.MealPlans.GetMealPlan;

// Complete meal plan ready to display to the user
public sealed record MealPlanResponse(
    Guid Id,
    string Name,
    DateOnly StartDate,
    int DuratioDays,
    decimal TotalCost,
    decimal Budget,
    string Currency,
    PlannerAlgorithm Algorithm,
    long GenerationMs,
    long ExploredNodes,
    double DiversityIndex,
    IReadOnlyList<MealPlanDayResponse> Days);

// One day of the plan
public sealed record MealPlanDayResponse(
    int DayNumber,
    DateOnly Date,
    int Calories,
    decimal Cost,
    IReadOnlyList<MealPlanEntryResponse> Meals);

// One meal plan entry
public sealed record MealPlanEntryResponse(
    MealType MealType,
    Guid DishId,
    string DishName,
    int Calories,
    decimal Cost,
    bool IsLocked);
