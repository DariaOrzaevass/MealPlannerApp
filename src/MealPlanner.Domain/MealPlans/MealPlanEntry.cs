using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Domain.MealPlans;

// One meal plan item
public sealed class MealPlanEntry : Entity<MealPlanEntryId>
{
    // Constructor to create a new MealPlanEntry
    internal MealPlanEntry(
    MealPlanEntryId id,
    MealPlanId mealPlanId,
    int dayNumber,
    MealType mealType,
    DishId dishId,
    Money cost)
    : base(id)
    {
        MealPlanId = mealPlanId;
        DayNumber = dayNumber;
        MealType = mealType;
        DishId = dishId;
        Cost = cost;
    }
    // Default constructor for EF core to get the Meal Plan Entry from the database
    private MealPlanEntry()
    {
    }

    // The Id of the specific Meal Plan
    public MealPlanId MealPlanId { get; private set; } = null!;

    // The number of the day the entry is supposed to go
    public int DayNumber { get; private set; }

    // The meal type of the entry (Breakfast, Lunch, etc.)
    public MealType MealType { get; private set; }

    // The Id of the specific dish
    public DishId DishId { get; private set; } = null!;

    // The cost of the specific dish in the moment of the creation of meal plan
    public Money Cost { get; private set; } = null!;
}
