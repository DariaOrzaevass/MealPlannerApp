using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Users;

namespace MealPlanner.Application.MealPlans.SetEntryLock;

// Lock or unlock the entry of the plan.
// Only one operation - setting the lock state
public sealed record SetEntryLockCommand(
    UserId UserId,
    Guid MealPlanId,
    int DayNumber,
    MealType MealType,
    bool Locked) : ICommand;
