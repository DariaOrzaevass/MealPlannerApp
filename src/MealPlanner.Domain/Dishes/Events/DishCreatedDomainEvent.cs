using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Dishes.Events;

// The dish has been created
public sealed record DishCreatedDomainEvent(DishId DishId) : DomainEvent;
