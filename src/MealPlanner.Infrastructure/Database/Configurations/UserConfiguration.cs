using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Users;
using MealPlanner.Infrastructure.Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Infrastructure.Database.Configurations;

// Configuration of the User entity for the Entity Framework Core.
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    // Configures the database mapping of the User entity
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Name of the table
        builder.ToTable("users");

        // Primary key
        builder.HasKey(u => u.Id);

        // Converts the UserId
        builder.Property(u => u.Id)
            .HasConversion<UserIdConverter>()
            .ValueGeneratedNever();

        // Configures required Email with max length of 256
        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        // Configures required IdentityId with max length of 450
        builder.Property(u => u.IdentityId)
            .HasMaxLength(450)
            .IsRequired();

        // The Email and the IdentityId of the
        // user must be unique in the database
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.IdentityId).IsUnique();
    }
}
