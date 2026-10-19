using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Infrastructure.Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Infrastructure.Database.Configurations;

// Configuration of the Ingredient entity for the Entity Framework Core.
internal sealed class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    // Configures the database mapping of the Ingredient entity
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        // Name of the table
        builder.ToTable("ingredients");

        // Primary key
        builder.HasKey(i => i.Id);

        // Converts the IngredientId
        builder.Property(i => i.Id)
            .HasConversion<IngredientIdConverter>()
            .ValueGeneratedNever();

        // Configures required Name with max length of 200
        builder.Property(i => i.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Configures required Category
        builder.Property(i => i.Category)
            .HasConversion<int>()
            .IsRequired();

        // Creates index on Name column
        builder.HasIndex(i => i.Name);

        // Each ingredient owns a collection of allergens and
        // this block creates a new table to represent that
        builder.OwnsMany(i => i.Allergens, allergens =>
        {
            // Name of the table for the ingredient's allergens
            allergens.ToTable("ingredient_allergens");

            // Foreign key that links to a specific ingredient in the "ingredients" table
            allergens.WithOwner().HasForeignKey(a => a.IngredientId);

            // Converts the IngredientId and the AllergenId
            allergens.Property(a => a.IngredientId).HasConversion<IngredientIdConverter>();
            allergens.Property(a => a.AllergenId).HasConversion<AllergenIdConverter>();

            // Creates composite primary key from the IngredientId and the AllergenId
            allergens.HasKey(a => new { a.IngredientId, a.AllergenId });
        });

        // Public property is read-only, so EF Core accesses
        // the private backing field directly instead of property
        builder.Navigation(i => i.Allergens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
