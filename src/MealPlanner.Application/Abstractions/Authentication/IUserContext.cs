using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Users;

namespace MealPlanner.Application.Abstractions.Authentication;

// User making the current request
public interface IUserContext
{
    Task<Result<UserId>> GetUserIdAsync(CancellationToken cancellationToken = default);
}
