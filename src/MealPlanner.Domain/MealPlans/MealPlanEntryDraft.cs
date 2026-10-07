using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Domain.MealPlans;

// The draft of the meal plan entry, something that algorithm returns. Needs to be checked
public readonly record struct MealPlanEntryDraft(
    int DayNumber,
    MealType MealType,
    DishId DishId,
    Money Cost);
