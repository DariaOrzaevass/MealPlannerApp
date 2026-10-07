using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.MealPlans.Events;

// The domain event for the generation of the meal plan
public sealed record MealPlanGeneratedDomainEvent(
    MealPlanId MealPlanId,
    PlannerAlgorithm Algorithm,
    long GenerationMs) : DomainEvent;
