using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Infrastructure.Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Infrastructure.Database.Configurations;

// Configuration of the Meal Plan entity for the Entity Framework Core.
internal sealed class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    // Configures the database mapping of the Meal Plan entity
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        // Name of the table
        builder.ToTable("meal_plans");

        // Primary key
        builder.HasKey(p => p.Id);

        // Converts the MealPlanId
        builder.Property(p => p.Id)
            .HasConversion<MealPlanIdConverter>()
            .ValueGeneratedNever();

        // Converts the UserId
        builder.Property(p => p.UserId)
            .HasConversion<UserIdConverter>()
            .IsRequired();
    }
}
