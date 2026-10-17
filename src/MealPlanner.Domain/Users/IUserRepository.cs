using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Users;

// Repository interface for User
public interface IUserRepository
{
    // For retrieving a user asynchronously by its unique identifier
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    // For retrieving a user asynchronously by its identity id
    Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default);

    // For adding a new user
    void Add(User user);
}
