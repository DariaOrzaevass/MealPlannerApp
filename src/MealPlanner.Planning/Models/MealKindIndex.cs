using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MealPlanner.Planning.Models;

// Helps to work with MealKinds
public static class MealKindIndex
{
    // How many meal kinds there are
    public const int Count = 4;

    // Checks whether kinds contains exactly one flag
    public static bool IsSingleKind(MealKinds kind) => BitOperations.PopCount((uint)kind) == 1;

    // Converts power-of-two flags into indexes
    public static int ToIndex(MealKinds kind)
    {
        if (!IsSingleKind(kind))
        {
            throw new ArgumentException("Exactly one meal kind was expected.", nameof(kind));
        }

        // How many zero bits are left on the right. Is equal to the
        // index. Breakfast - 0, Lunch - 1, Dinner - 2, Snack - 3
        return BitOperations.TrailingZeroCount((uint)kind);
    }

    // Reverse of ToIndex
    public static MealKinds FromIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

        return (MealKinds)(1 << index);
    }
}
