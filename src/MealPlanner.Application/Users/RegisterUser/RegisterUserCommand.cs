using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;

namespace MealPlanner.Application.Users.RegisterUser;

// Record representing a command for registering a new user
public sealed record RegisterUserCommand(string Email, string Password) : ICommand<Guid>;
