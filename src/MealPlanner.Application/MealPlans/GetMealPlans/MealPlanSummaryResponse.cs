using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.MealPlans;

namespace MealPlanner.Application.MealPlans.GetMealPlans;

// The the meal plan item in the list. Only neccessary fields
// that are displayed. No meal plan entries, because loading
// them means reading hundrets of rows without even displaying
public sealed record MealPlanSummaryResponse(
    Guid Id,
    string Name,
    DateOnly StartDate,
    int DurationDays,
    decimal TotalCost,
    decimal Budget,
    string Currency,
    PlannerAlgorithm Algorithm,
    long GenerationMs,
    DateTime CreatedOnUtc);
