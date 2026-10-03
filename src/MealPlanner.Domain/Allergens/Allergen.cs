using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Allergens;

// An allergen entity representing specific allergen
public sealed class Allergen : Entity<AllergenId>
{
    // Default constructor for EF core to get allergen from the database
    private Allergen()
    {
    }

    // The name of the allergen like "Peanuts" to display to the user
    public string Name { get; private set; } = null!;

    // The unique code of the allergen like "PEANUTS" for internal use
    public string Code { get; private set; } = null!;

    // The bit position of the specific allergen in the bitmask
    // from 0 to 63 for the filtration of allergens in the meal plan
    public int BitPosition { get; private set; }

    // Mask with a single set bit in the unique for the allergen position
    public ulong Mask => 1UL << BitPosition;
}
