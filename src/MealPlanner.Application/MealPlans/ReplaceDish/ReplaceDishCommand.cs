using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Users;

namespace MealPlanner.Application.MealPlans.ReplaceDish;

// Replace the dish in one slot of the saved meal plan
public sealed record ReplaceDishCommand(
    UserId UserId,
    Guid MealPlanId,
    int DayNumber,
    MealType MealType,
    IReadOnlyList<Guid> RejectedDishes) : ICommand<ReplaceDishResponse>;
