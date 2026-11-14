using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Authentication;

// The database context used specifically for the ASP.NET Core Identity
public sealed class ApplicationIdentityDbContext(
    DbContextOptions<ApplicationIdentityDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    // Defines the schema in which Identity tables will be located
    public const string Schema = "identity";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Schema);

        // For the consistency default ASP.NET Core Identity tables are renamed
        builder.Entity<IdentityUser>().ToTable("users");
        builder.Entity<IdentityRole>().ToTable("roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
        builder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");
    }
}
