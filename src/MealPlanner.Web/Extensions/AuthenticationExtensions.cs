using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Domain.Users;
using MealPlanner.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Web.Extensions;

// Configures authentification and authorization for the Web layer
public static class AuthenticationExtensions
{
    public static IServiceCollection AddApplicationAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Identity is used without its built-in Razor pages.
        // Login and registration use custom endpoints.
        // Identity UI would bring many unnecessary pages.
        services.AddIdentityCore<IdentityUser>(options =>
        {
            options.User.RequireUniqueEmail = true;

            // Password must contain at least 8 characters, upper+lower case,
            // and a number. Requiring a special character usually only
            // leads to predictable passwords like "Password1!".
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            // Default paths point to Identity pages like (/Account/Login),
            // but those pages do not exist because the built-in Identity
            // UI isn't used. Application specifies its own routes.
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/denied";
            options.ReturnUrlParameter = "returnUrl";

            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;

            // Redirecting API request to a login page is meaningless. Client
            // expects HTTP status code but can instead receive HTML page. Redirects
            // stay enabled for the rest of the application, including Blazor pages.
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);

                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);

                return Task.CompletedTask;
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Administrator, policy =>
                policy.RequireRole(Roles.Administrator));

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
}
