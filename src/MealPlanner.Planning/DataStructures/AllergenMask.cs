using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MealPlanner.Planning.DataStructures;

// A bit mask representing allergens and methods to work with it
public readonly record struct AllergenMask(ulong Bits)
{
    // How many allergens could there be in the mask
    public const int Capacity = 64;

    // New empty allergen mask. UL to use unsigned long specifically
    public static readonly AllergenMask Empty = new(0UL);

    // Answers "is it empty?"
    public bool IsEmpty => Bits == 0UL;

    // Answers how many allergens are in the mask. Is counted by the number of 1
    public int Count => BitOperations.PopCount(Bits);

    // Creates an allergen mask with one bit for the particular allergen set to one\
    // Checks to not ot create same mask for different allergens (shift by 64 = shift by 0)
    public static AllergenMask FromPosition(int position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(position, Capacity);

        // 1UL is ..00001 and << position shifts left 1 to the required position
        return new AllergenMask(1UL << position);
    }

    // Creates an allergen mask from multiple positions
    public static AllergenMask FromPositions(ReadOnlySpan<int> positions)
    {
        ulong bits = 0UL;

        foreach (int position in positions)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(position);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(position, Capacity);

            // Uses bitwise OR to store the result in bits
            bits |= 1UL << position;
        }

        // Returns finished mask in the end
        return new AllergenMask(bits);
    }

    // Does it contains an allergen with this position
    public bool Contains(int position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(position, Capacity);

        // Uses bitwise AND to check if the bit is set to one.
        // If it is the mask will not be equal to 0
        return (Bits & (1UL << position)) != 0UL;
    }

    // Returns an existing mask with the new added allergen
    public AllergenMask With(int position) => this | FromPosition(position);

    // Do two allergen masks have at least one allegren in common
    public bool Intersects(AllergenMask other) => (Bits & other.Bits) != 0UL;

    // Bitwise OR to combine two masks in one
    public static AllergenMask operator |(AllergenMask left, AllergenMask right) =>
    new(left.Bits | right.Bits);

    // Bitwise AND for checking bitmasks and comparing them
    public static AllergenMask operator &(AllergenMask left, AllergenMask right) =>
    new(left.Bits & right.Bits);

    // Returns positions of allegrens present in the bitmask
    public int[] ToPositions()
    {
        if (IsEmpty)
        {
            return [];
        }

        int[] positions = new int[Count];

        // Copy of the bitmask
        ulong rest = Bits;
        int written = 0;
        while (rest != 0UL)
        {
            // How many trailing 0 after 1
            positions[written++] = BitOperations.TrailingZeroCount(rest);
            // Removes the lowest set bit by bitwise AND
            rest &= rest - 1UL;
        }

        return positions;
    }

    // Overrides ToString to return all allergens present in the bitmask
    public override string ToString() =>
    IsEmpty ? "{}" : $"{{{string.Join(", ", ToPositions())}}}";
}
