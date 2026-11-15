using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Application.Abstractions.Messaging;

// Command that does not return a value
public interface ICommand : IBaseCommand;

// Command that returns a value
public interface ICommand<TResponse> : IBaseCommand;

// Common parent interface for both types of commands
public interface IBaseCommand;
