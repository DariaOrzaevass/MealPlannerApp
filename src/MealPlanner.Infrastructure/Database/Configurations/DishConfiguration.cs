using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Dishes;
using MealPlanner.Infrastructure.Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Infrastructure.Database.Configurations;

// Configuration of the Dish entity for the Entity Framework Core.
internal sealed class DishConfiguration : IEntityTypeConfiguration<Dish>
{
    // Configures the database mapping of the Dish entity
    public void Configure(EntityTypeBuilder<Dish> builder)
    {
        // Name of the table
        builder.ToTable("dishes");

        // Primary key
        builder.HasKey(d => d.Id);

        // Converts the DishId
        builder.Property(d => d.Id)
            .HasConversion<DishIdConverter>()
            .ValueGeneratedNever();

        // Configures required Name with max length 200
        builder.Property(d => d.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Configures Description with max length of 256
        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        // Configures required Recipe with no
        // max length, since recipes can be long
        builder.Property(d => d.Recipe)
            .IsRequired();

        // Money is the value object that doesn't need its own table.
        // OwnsOne will put it in the dish table in two columns
        builder.OwnsOne(d => d.Cost, cost =>
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
        builder.Navigation(d => d.Cost).IsRequired();

        // Creates table columns from Dish properties
        builder.Property(d => d.Calories).IsRequired();
        builder.Property(d => d.SuitableFor).HasConversion<int>().IsRequired();
        builder.Property(d => d.CookingTimeMinutes).IsRequired();
        builder.Property(d => d.Diet).HasConversion<int>().IsRequired();
        builder.Property(d => d.Cuisine).HasConversion<int>().IsRequired();
        builder.Property(d => d.IsPublished).IsRequired();

        // Creates index on IsPublished column
        // for easier search for published dishes
        builder.HasIndex(d => d.IsPublished);

        // Each dish owns a collection of ingredients and
        // this block creates a new table to represent that
        builder.OwnsMany(d => d.Ingredients, ingredients =>
        {
            // Name of the table for the ingredients of the dish
            ingredients.ToTable("dish_ingredients");

            // Foreign key that links to a specific dish in the "dishes" table
            ingredients.WithOwner().HasForeignKey(i => i.DishId);

            // Converts the IngredientId and the DishId
            ingredients.Property(i => i.DishId).HasConversion<DishIdConverter>();
            ingredients.Property(i => i.IngredientId).HasConversion<IngredientIdConverter>();

            // Creates table columns for Quantity (20) and Measurement units (grams) of the ingredient
            ingredients.Property(i => i.Quantity).HasColumnType("decimal(10,2)");
            ingredients.Property(i => i.Unit).HasConversion<int>();

            // Creates composite primary key from the IngredientId and the DishId
            ingredients.HasKey(i => new { i.DishId, i.IngredientId });
        });

        // Public property is read-only, so EF Core accesses
        // the private backing field directly instead of property
        builder.Navigation(d => d.Ingredients)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
