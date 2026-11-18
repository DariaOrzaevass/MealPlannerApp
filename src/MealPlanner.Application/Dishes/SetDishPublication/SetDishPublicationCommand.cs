using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;

namespace MealPlanner.Application.Dishes.SetDishPublication;

// Publishes or unpublishes a dish. A single command for both actions, only
// one operation "set the publication state", actions only differ by the value of
// the flag. Two separate commands would need two handlers with difference in one line
public sealed record SetDishPublicationCommand(Guid DishId, bool Published) : ICommand;
