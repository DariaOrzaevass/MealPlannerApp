using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Allergens;

// An allergen entity representing specific allergen
public sealed class Allergen : Entity<AllergenId>
{
    // Private constructor so that the allergen can only be created using the Create method with validation
    private Allergen(AllergenId id, string name, string code, int bitPosition)
    : base(id)
    {
        Name = name;
        Code = code;
        BitPosition = bitPosition;
    }

    // Default constructor for EF core to get allergen from the database
    private Allergen()
    {
    }

    // Maximum number of allergen in the application
    public const int MaxAllergenCount = 64;

    // The name of the allergen like "Peanuts" to display to the user
    public string Name { get; private set; } = null!;

    // The unique code of the allergen like "PEANUTS" for internal use
    public string Code { get; private set; } = null!;

    // The bit position of the specific allergen in the bitmask
    // from 0 to 63 for the filtration of allergens in the meal plan
    public int BitPosition { get; private set; }

    // Mask with a single set bit in the unique for the allergen position
    public ulong Mask => 1UL << BitPosition;

    // The method to return a new allergen with checks for any errors in the input
    public static Result<Allergen> Create(string name, string code, int bitPosition)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Allergen>(AllergenErrors.EmptyName);
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Allergen>(AllergenErrors.EmptyCode);
        }

        if (bitPosition is < 0 or >= MaxAllergenCount)
        {
            return Result.Failure<Allergen>(AllergenErrors.BitPositionOutOfRange);
        }

        // Only after all checks are passed the new allergen can be created and returned
        return new Allergen(AllergenId.New(), name.Trim(), code.Trim().ToUpperInvariant(), bitPosition);
    }
}
