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

        // Configures required Name with max length 200
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Configures required StartDate converting
        // DateOnly to date in the database
        builder.Property(p => p.StartDate)
            .HasColumnType("date")
            .IsRequired();

        // Converts the PlanningConstraints to JSON
        builder.Property(p => p.Constraints)
            .HasConversion<PlanningConstraintsConverter>()
            .HasColumnName("constraints")
            .HasColumnType("nvarchar(max)")
            .Metadata.SetValueComparer(PlanningConstraintsConverter.Comparer);

        // Money is the value object that doesn't need its own table.
        // OwnsOne will put TotalCost in the meal_plans table in two columns
        builder.OwnsOne(p => p.TotalCost, cost =>
        {
            // Column with a decimal amount of a total cost
            cost.Property(m => m.Amount)
                .HasColumnName("total_cost_amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Column with a currency of a total cost
            cost.Property(m => m.Currency)
                .HasColumnName("total_cost_currency")
                .HasConversion<CurrencyConverter>()
                .HasMaxLength(3)
                .IsRequired();
        });

        // The meal plan must always has total cost
        builder.Navigation(p => p.TotalCost).IsRequired();

        // Creates table columns from MealPlan properties
        builder.Property(p => p.Algorithm).HasConversion<int>().IsRequired();
        builder.Property(p => p.GenerationMs).IsRequired();
        builder.Property(p => p.ExploredNodes).IsRequired();
        builder.Property(p => p.CreatedOnUtc).IsRequired();

        // The list of the user's meal plans is the most popular query
        builder.HasIndex(p => new { p.UserId, p.CreatedOnUtc });

        // Each meal plan owns a collection of meal plan entries
        // and this block creates a new table to represent that
        builder.OwnsMany(p => p.Entries, entries =>
        {
            // Name of the table for the meal plan entries
            entries.ToTable("meal_plan_entries");

            // Foreign key that links entry to a specific Meal Plan
            entries.WithOwner().HasForeignKey(e => e.MealPlanId);

            // Primary key
            entries.HasKey(e => e.Id);

            // Converts the MealPlanEntryId
            entries.Property(e => e.Id)
                .HasConversion<MealPlanEntryIdConverter>()
                .ValueGeneratedNever();

            // Converts the MealPlanId and DishId
            entries.Property(e => e.MealPlanId).HasConversion<MealPlanIdConverter>();
            entries.Property(e => e.DishId).HasConversion<DishIdConverter>().IsRequired();

            // Creates table columns for DayNumber, MealType, and IsLocked
            entries.Property(e => e.DayNumber).IsRequired();
            entries.Property(e => e.MealType).HasConversion<int>().IsRequired();
            entries.Property(e => e.IsLocked).IsRequired();

            // OwnsOne will put Cost in the dish table in two columns
            entries.OwnsOne(e => e.Cost, cost =>
            {
                // Column with a decimal amount of a dish cost
                cost.Property(m => m.Amount)
                    .HasColumnName("cost_amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                // Column with a currency of a dish cost
                cost.Property(m => m.Currency)
                    .HasColumnName("cost_currency")
                    .HasConversion<CurrencyConverter>()
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // The dish must always has some cost
            entries.Navigation(e => e.Cost).IsRequired();

            // Makes sure that MealPlanId, DayNumber, and MealType
            // combinations are uniwue for each entry in the table
            entries.HasIndex(e => new { e.MealPlanId, e.DayNumber, e.MealType }).IsUnique();
        });

        // Public property is read-only, so EF Core accesses
        // the private backing field directly instead of property
        builder.Navigation(p => p.Entries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
