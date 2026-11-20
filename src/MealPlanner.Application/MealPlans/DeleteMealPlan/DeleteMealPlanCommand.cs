using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Users;
using MealPlanner.Application.Abstractions.Messaging;

namespace MealPlanner.Application.MealPlans.DeleteMealPlan;

// Delets the saved meal plan. Does not return any response. UserId is supplied by the
// endpoint, not the client, so deleting another user's meal plan would not be possible.
public sealed record DeleteMealPlanCommand(UserId UserId, Guid MealPlanId) : ICommand;
