using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Users;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Application.Users.RegisterUser;

// Handler for RegisterUserCommand
internal sealed class RegisterUserCommandHandler(
    IIdentityProvider identity,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    ILogger<RegisterUserCommandHandler> logger
    ) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        string email = command.Email.Trim().ToLowerInvariant();

        Result<string> identityId = await identity.CreateAsync(
           email,
           command.Password,
           Domain.Users.Roles.Member,
           cancellationToken);

        if (identityId.IsFailure)
        {
            return Result.Failure<Guid>(identityId.Error);
        }

        Result<User> user = User.Create(email, identityId.Value);

        if (user.IsFailure)
        {
            await CompensateAsync(identityId.Value, cancellationToken);
            return Result.Failure<Guid>(user.Error);
        }

        users.Add(user.Value);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Failed to save the user after registration.");
            await CompensateAsync(identityId.Value, cancellationToken);
            throw;
        }

        return user.Value.Id.Value;
    }

    private async Task CompensateAsync(string identityId, CancellationToken cancellationToken)
    {
        try
        {
            await identity.DeleteAsync(identityId, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(
                exception,
                "Failed to delete Identity account {IdentityId} after unsuccessful registration. " +
                "The account remains without a corresponding domain user.",
                identityId);
        }
    }
}
