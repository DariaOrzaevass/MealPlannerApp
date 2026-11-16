namespace MealPlanner.Web.Contracts;

// Registration data.
public sealed record RegisterRequest(string Email, string Password);

// Login data. RememberMe specifies whether the login should be stored between
// browser sessions. Without it cookie lasts only until the browser is closed.
public sealed record LoginRequest(string Email, string Password, bool RememberMe = false);

// Information about the current user.
public sealed record CurrentUserResponse(string Email, IReadOnlyList<string> Roles);
