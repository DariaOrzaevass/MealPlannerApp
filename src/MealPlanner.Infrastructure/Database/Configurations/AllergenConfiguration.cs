using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Allergens;
using MealPlanner.Infrastructure.Database.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Infrastructure.Database.Configurations;

// Configuration of the Allergen entity for the Entity Framework Core.
internal sealed class AllergenConfiguration : IEntityTypeConfiguration<Allergen>
{
    // Configures the database mapping of the Allergen entity
    public void Configure(EntityTypeBuilder<Allergen> builder)
    {
        // Name of the table
        builder.ToTable("allergens");

        // Primary key
        builder.HasKey(a => a.Id);

        // Converts the AllergenId
        builder.Property(a => a.Id)
            .HasConversion<AllergenIdConverter>()
            .ValueGeneratedNever();

        // Configures required Name with max length of 100
        builder.Property(a => a.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Configures required Code with max length of 32
        builder.Property(a => a.Code)
            .HasMaxLength(32)
            .IsRequired();

        // Configures required BitPosition
        builder.Property(a => a.BitPosition)
            .IsRequired();

        // The Code and the BitPosition of the
        // allergen must be unique in the database
        builder.HasIndex(a => a.Code).IsUnique();
        builder.HasIndex(a => a.BitPosition).IsUnique();
    }
}
