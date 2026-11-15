using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Application.Abstractions.Decorators;

// Adds logging around command and query handlers so that every handler doesn't need
// to contain its own logging. Logging is registered last, so it becomes the outer
// layer around validation. This means validation failures are logged too. Otherwise
// failed requests will look like they never reached the application.
internal static class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string name = typeof(TCommand).Name;

            logger.LogInformation("Processing command {Request}", name);

            Result result = await innerHandler.Handle(command, cancellationToken);

            LogCompletion(logger, name, result);

            return result;
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(
            TCommand command,
            CancellationToken cancellationToken)
        {
            string name = typeof(TCommand).Name;

            logger.LogInformation("Processing command {Request}", name);

            Result<TResponse> result = await innerHandler.Handle(command, cancellationToken);

            LogCompletion(logger, name, result);

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(
            TQuery query,
            CancellationToken cancellationToken)
        {
            string name = typeof(TQuery).Name;

            logger.LogInformation("Processing query {Request}", name);

            Result<TResponse> result = await innerHandler.Handle(query, cancellationToken);

            LogCompletion(logger, name, result);

            return result;
        }
    }

    // Successfull completion is not logged separately -
    // it's already visible in web server logs
    private static void LogCompletion(ILogger logger, string name, Result result)
    {
        if (result.IsSuccess)
        {
            return;
        }

        // "Not Found" is expected if user requests a plan that does not
        // belong to them. Such case is logged as warning, not error that
        // require alert. Frequent "not found" alerts will be ignored soon.
        if (result.Error.Type is ErrorType.NotFound)
        {
            logger.LogWarning(
                "Request {Request} is denied {Code}: {Description}",
                name,
                result.Error.Code,
                result.Error.Description);

            return;
        }

        logger.LogError(
            "Request {Request} completed with error {Code}: {Description}",
            name,
            result.Error.Code,
            result.Error.Description);
    }
}
