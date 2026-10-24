using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Models;

// Represents days on which the dish has been used in the plan
public static class DayMask
{
    // maximum amount of days that can go into bitmask
    public const int MaxDays = 64;

    // Mask for one day
    public static ulong OfDay(int day)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(day, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(day, MaxDays);

        // Days start with 1 (Day 1), but position starts with 0
        return 1UL << (day - 1);
    }

    // Creates a mask for all days when the dish cannot be used again
    public static ulong ForbiddenWindow(int day, int gap, int durationDays)
    {
        // If gap is 0 - no restrictions
        if (gap <= 0)
        {
            return 0UL;
        }

        // the beginning and ending of the forbidden window of days.
        // Cannot have less then 1 days or longer than the plan duration
        int from = Math.Max(1, day - gap + 1);
        int to = Math.Min(durationDays, day + gap - 1);

        // Checks that the forbidden window is valid
        if (from > to)
        {
            return 0UL;
        }

        // Includes both end points so + 1
        int length = to - from + 1;

        // Creates mask of length consecutive 1-bits. If length is = the maximum
        // amount of days it would just give 1, but we need ulong.MaxValue
        ulong ones = length >= MaxDays ? ulong.MaxValue : (1UL << length) - 1UL;

        // Shifts the mask to the required position
        return ones << (from - 1);
    }

    // Checks whether the previously used days overlap with the forbidden window, violating it
    public static bool ViolatesGap(ulong usedDays, int day, int gap, int durationDays) =>
        (usedDays & ForbiddenWindow(day, gap, durationDays)) != 0UL;
}
