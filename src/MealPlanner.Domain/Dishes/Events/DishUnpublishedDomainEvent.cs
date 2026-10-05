using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Dishes.Events;

// The dish is unpublished and cannot be included in the meal plan generatio process
public sealed record DishUnpublishedDomainEvent(DishId DishId) : DomainEvent;
