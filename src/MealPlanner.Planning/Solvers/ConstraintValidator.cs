using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Solvers;

// Checks whether a finished meal plan fits hard constraints
public static class ConstraintValidator
{
    // Returns the first violation it found or none
    public static ConstraintViolation Validate(
    PlanningProblem problem,
    ReadOnlySpan<int> assignment)
    {
        ArgumentNullException.ThrowIfNull(problem);

        PlanningSpec spec = problem.Spec;
        DishCatalog catalog = problem.Catalog;

        // Checks that there is one dish assigned to every slot
        if (assignment.Length != spec.SlotCount)
        {
            return ConstraintViolation.Incomplete;
        }

        // How many times the dish was used and on which days(bitmask)
        var useCount = new int[catalog.Count];
        var usedDays = new ulong[catalog.Count];

        //Calories for each day separately(day 1, day 2 so spec.DurationDays + 1)
        var caloriesPerDay = new int[spec.DurationDays + 1];
        //long totalCost = 0;

        // Processes all meal plan slots
        for (int slotIndex = 0; slotIndex < assignment.Length; slotIndex++)
        {
            // Get the dish assigned for this slot
            int dish = assignment[slotIndex];

            // Checks the validity od the dish id
            if (dish < 0 || dish >= catalog.Count)
            {
                return ConstraintViolation.Incomplete;
            }

            // Get iformation about the slot like day and meal type
            SlotDescriptor slot = spec.Slots[slotIndex];

            // Checks if the pinned slot contains the pinned dish
            if (spec.IsPinned(slotIndex) && spec.PinnedSlots[slotIndex] != dish)
            {
                return ConstraintViolation.Pinned;
            }

            // Checks if the dish is a part of the excluded dishes in the plan regeneration
            // The dish can be pinned in some slots and regected in all others so don't check pinned
            if (!spec.IsPinned(slotIndex) && spec.ExcludedDishes.Contains(dish))
            {
                return ConstraintViolation.Excluded;
            }

            // Checks if the dish contains one of user's allergen
            if (catalog.Allergens[dish].Intersects(spec.ExcludedAllergens))
            {
                return ConstraintViolation.Allergen;
            }

            // Checks if the dish contains one of user's disliked ingredient
            foreach (int ingredient in catalog.IngredientsOf(dish))
            {
                if (spec.DislikedIngredients.Contains(ingredient))
                {
                    return ConstraintViolation.DislikedIngredient;
                }
            }

            // Checks if the dish does not fit user's diet
            if (catalog.DietLevels[dish] < spec.DietLevel)
            {
                return ConstraintViolation.Diet;
            }

            // Checks if the cooking time of the dish is more than allowed
            if (catalog.CookingTimeMinutes[dish] > spec.MaxCookingTimeMinutes)
            {
                return ConstraintViolation.CookingTime;
            }
        }

        return ConstraintViolation.None;
    }

}
