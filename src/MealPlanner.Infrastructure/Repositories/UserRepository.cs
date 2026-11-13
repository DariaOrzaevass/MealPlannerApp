using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Users;
using MealPlanner.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Repositories;

// Repository for the User entity
internal sealed class UserRepository(ApplicationDbContext context)
    : Repository<User, UserId>(context), IUserRepository
{
    // Finds a user by their Identity ID
    public async Task<User?> GetByIdentityIdAsync(
        string identityId,
        CancellationToken cancellationToken = default) =>
        await Context.Set<User>()
            .FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
}
