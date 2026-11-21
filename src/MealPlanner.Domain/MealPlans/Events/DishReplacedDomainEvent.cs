using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Domain.MealPlans.Events;

// User replaced a dish in the complete plan
public sealed record DishReplacedDomainEvent(
    MealPlanId MealPlanId,
    int DayNumber,
    MealType MealType,
    DishId OldDishId,
    DishId NewDishId) : DomainEvent;
