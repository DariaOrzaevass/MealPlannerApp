using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Allergens;

// Class containing all errors related to allergens
public static class AllergenErrors
{
    // Returns an error if the allergen with this ID is not found
    public static Error NotFound(AllergenId id) =>
        Error.NotFound("Allergen.NotFound", $"Allergen with ID {id} not found.");

    // Returns an error if the name of the allergen is empty
    public static readonly Error EmptyName =
    Error.Validation("Allergen.EmptyName", "Allergen name cannot be empty.");

    // Returns an error if the code of the allergen is empty
    public static readonly Error EmptyCode =
    Error.Validation("Allergen.EmptyCode", "Allergen code cannot be empty.");

    // Returns an error if the bit position of the allergen is out of the range
    public static readonly Error BitPositionOutOfRange =
        Error.Validation(
            "Allergen.BitPositionOutOfRange",
            $"Position of the bit must be between 0 and {Allergen.MaxAllergenCount - 1}.");

}
