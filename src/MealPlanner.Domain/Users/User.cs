using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Users;

public sealed class User : Entity<UserId>
{
    // Private constructor so that the user can only
    // be created using the Create method with validation
    private User(UserId id, string email, string identityId)
    : base(id)
    {
        Email = email;
        IdentityId = identityId;
    }

    // Default constructor for EF core to get the user from the database
    private User()
    {
    }

    // Email of the user
    public string Email { get; private set; } = null!;

    // Links the user entity in the domain with the user in Identity.
    // Firstly the user is created in the Identity
    public string IdentityId { get; private set; } = null!;

    // Creates a new user after validating the input
    public static Result<User> Create(string email, string identityId)
    {
        // Checks that the email is not empty
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User>(UserErrors.EmptyEmail);
        }

        // Checks that the IdentityId is not empty
        if (string.IsNullOrWhiteSpace(identityId))
        {
            return Result.Failure<User>(UserErrors.EmptyIdentityId);
        }

        // Creates a user with a new domain id, the identityId, and lowercase email
        return new User(UserId.New(), email.Trim().ToLowerInvariant(), identityId);
    }
}
