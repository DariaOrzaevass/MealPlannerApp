using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using FluentValidation.Results;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Application.Abstractions.Decorators;

// Adds input validation around command and query handlers
internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            ValidationFailure[] failures = await ValidateAsync(command, validators);

            return failures.Length == 0
                ? await innerHandler.Handle(command, cancellationToken)
                : Result.Failure(CreateValidationError(failures));
        }
    }

    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        IEnumerable<IValidator<TCommand>> validators)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(
            TCommand command,
            CancellationToken cancellationToken)
        {
            ValidationFailure[] failures = await ValidateAsync(command, validators);

            return failures.Length == 0
                ? await innerHandler.Handle(command, cancellationToken)
                : Result.Failure<TResponse>(CreateValidationError(failures));
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        IEnumerable<IValidator<TQuery>> validators)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(
            TQuery query,
            CancellationToken cancellationToken)
        {
            ValidationFailure[] failures = await ValidateAsync(query, validators);

            return failures.Length == 0
                ? await innerHandler.Handle(query, cancellationToken)
                : Result.Failure<TResponse>(CreateValidationError(failures));
        }
    }

    private static async Task<ValidationFailure[]> ValidateAsync<T>(
        T request,
        IEnumerable<IValidator<T>> validators)
    {
        // There maybe no validators. In that case don't
        // create validation context or any tasks
        IValidator<T>[] all = [.. validators];

        if (all.Length == 0)
        {
            return [];
        }

        var context = new ValidationContext<T>(request);

        ValidationResult[] results =
            await Task.WhenAll(all.Select(v => v.ValidateAsync(context)));

        return [.. results.Where(r => !r.IsValid).SelectMany(r => r.Errors)];
    }

    // All violations of the validation are collected into one
    // composite error. It is more convenient to see them all together
    private static ValidationError CreateValidationError(ValidationFailure[] failures) =>
        new([.. failures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage))]);
}
