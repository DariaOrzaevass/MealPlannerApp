using System.Security.Claims;
using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Users;
using Microsoft.AspNetCore.Components.Authorization;

namespace MealPlanner.Web.Extensions;

// Determines the current user based on HTTP request or Blazor authentication state.
internal sealed class UserContext(
    IHttpContextAccessor accessor,
    AuthenticationStateProvider authenticationState,
    IUserRepository users) : IUserContext
{
    private UserId? _cached;

    public async Task<Result<UserId>> GetUserIdAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cached is not null)
        {
            return Result.Success(_cached);
        }

        string? identityId = await FindIdentityIdAsync();

        if (string.IsNullOrEmpty(identityId))
        {
            return Result.Failure<UserId>(UserErrors.NotAuthenticated);
        }

        User? user = await users.GetByIdentityIdAsync(identityId, cancellationToken);

        if (user is null)
        {
            // There is an Identity account, but no corresponding
            // domain user - the case of incomplete registration.
            return Result.Failure<UserId>(UserErrors.NotAuthenticated);
        }

        _cached = user.Id;

        return Result.Success(_cached);
    }

    private async Task<string?> FindIdentityIdAsync()
    {
        ClaimsPrincipal? principal = accessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated == true)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        AuthenticationState state = await authenticationState.GetAuthenticationStateAsync();

        return state.User.Identity?.IsAuthenticated == true
            ? state.User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
    }
}
