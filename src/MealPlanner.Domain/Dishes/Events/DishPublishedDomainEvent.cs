using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Dishes.Events;

// The dish is published and can be included in the meal plan generatio process
public sealed record DishPublishedDomainEvent(DishId DishId) : DomainEvent;
