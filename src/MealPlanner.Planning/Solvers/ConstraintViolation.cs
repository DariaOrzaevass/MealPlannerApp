using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Solvers;

// Different types of constraints violation
public enum ConstraintViolation
{
    None = 0,

    // the number of assigned positions is not equal to
    // meal plan slots, or some slot has no dish assigned
    Incomplete,

    // The dish does not fit this meal type
    MealType,

    // The dish contains one of user's allergen
    Allergen,

    // The dish contains one of user's disliked ingredient
    DislikedIngredient,

    // The dish does not fit user's diet
    Diet,

    // The cooking time is more than allowed
    CookingTime,

    // The total cost of the meal plan exceed the budget
    Budget,

    // The dish is repeated more times than allowed
    MaxRepeats,

    // The repeat gap between using the same dish is less than allowed
    RepeatGap,

    // Total calories for this day are not in the allowed range
    DailyCalories,

    // The pinned slot contains another dish that is pinned to it
    Pinned,

    // The dish is a part of the excluded dishes in the plan regeneration
    Excluded
}
