using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Solvers;

// The state of the search
internal sealed class SearchState
{
    // Empty meal slot
    private const int _unassigned = -1;

    // Stores the complete planning problem
    private readonly PlanningProblem _problem;

    // Which dish is assigned to which slot
    private readonly int[] _assignment;

    // How many time each dish appears
    private readonly int[] _useCount;

    // Stores a DayMask for every dish (when it was used)
    private readonly ulong[] _usedDays;

    // Stores total calories per day
    private readonly int[] _caloriesPerDay;

    // How many meal slots for this day have benn assigned
    private readonly int[] _assignedPerDay;

    // How many slots the day has
    private readonly int[] _slotsPerDay;

    // The sum of the average costs for unassigned slots. Used as the
    // denominator in calculating slot's share of the budget (remaining)
    private double _remainingWeight;

    // The sum of the minimum costs for unassigned
    // slots. Used as the lower bound for pruning
    private int _remainingMinCost;

    // Creates a search state
    public SearchState(PlanningProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        _problem = problem;

        // Creates an assignment array filled with -1
        _assignment = new int[problem.SlotCount];
        Array.Fill(_assignment, _unassigned);

        // Creates arrays for other fields.
        // DurationDays + 1 to correctly refer to all days
        _useCount = new int[problem.Catalog.Count];
        _usedDays = new ulong[problem.Catalog.Count];
        _caloriesPerDay = new int[problem.Spec.DurationDays + 1];
        _assignedPerDay = new int[problem.Spec.DurationDays + 1];
        _slotsPerDay = new int[problem.Spec.DurationDays + 1];

        // Loops through all slots and calculates remainingWeight and remainingMinCost
        // for all unassigned slots (now all slots) and how many meal slots each day has
        foreach (SlotDescriptor slot in problem.Spec.Slots)
        {
            _remainingWeight += problem.Domains.AverageCostForKind(slot.Kind);
            _remainingMinCost += problem.Domains.MinCostForKind(slot.Kind);
            _slotsPerDay[slot.Day]++;
        }
    }

    // The current assignments: the id of the dish or -1
    public ReadOnlySpan<int> Assignment => _assignment;

    // How much is spend in Minor Units (cents)
    public int SpentMinorUnits { get; private set; }

    // How many slots are assigned currently
    public int AssignedCount { get; private set; }

    // How many distinct dishes are used. Needed for a
    // lower bound of the variety term in branch and bound
    public int DistinctDishCount { get; private set; }

    // Stores the sum of the preference penalties
    public double PreferenceSum { get; private set; }

    // Similarity for assigned from both sides neighbouring slots.
    // Unnassigned slots are not considered. Similarities are not
    // negative, so the current sum can be used as a lower bound
    public double MonotonySum { get; private set; }

    // Answers the question "is the plan complete?"
    public bool IsComplete => AssignedCount == _assignment.Length;

    public int UseCount(int dish) => _useCount[dish];

    public int CaloriesOfDay(int day) => _caloriesPerDay[day];

    // Returns whether all slots of the day are assigned. Only after full assignment the
    // day calorie number can be checked against the minimum daily calories allowed
    public bool IsDayComplete(int day) => _assignedPerDay[day] == _slotsPerDay[day];

    public int RemainingMinCost => _remainingMinCost;

    // Checks whether it possible to place a dish to a meal slot
    // without violation of maximum amount of repeats, the gap
    // between repeats and the maximum daily calorie number
    public bool CanPlace(SlotDescriptor slot, int dish)
    {
        if (_useCount[dish] >= _problem.Spec.MaxRepeats)
        {
            return false;
        }

        if (ViolatesGap(slot, dish))
        {
            return false;
        }

        return !ExceedsDailyCalories(slot, dish);
    }

    // Checks whether the allowed gap between repeats is violated
    public bool ViolatesGap(SlotDescriptor slot, int dish) =>
        DayMask.ViolatesGap(
            _usedDays[dish],
            slot.Day,
            _problem.Spec.MinDaysBetweenRepeats,
            _problem.Spec.DurationDays);

    // Checks whether the dish assignment will exceed the maximum allowed daily calories
    public bool ExceedsDailyCalories(SlotDescriptor slot, int dish) =>
        _caloriesPerDay[slot.Day] + _problem.Catalog.Calories[dish] > _problem.Spec.MaxDailyCalories;

    // Checks whether the assigned dish can be replaced with other dish
    public bool CanReplace(SlotDescriptor slot, int candidate)
    {
        // Gets the current dish
        int current = _assignment[slot.Index];

        // No point to replace a dish with itself
        if (current == candidate)
        {
            return false;
        }

        // Checks that maximum amount of repeats is not violated
        if (_useCount[candidate] >= _problem.Spec.MaxRepeats)
        {
            return false;
        }

        // Checks that the gap between repeats is not violated
        if (ViolatesGap(slot, candidate))
        {
            return false;
        }

        // Checks that maximum daily calorie
        // number is not violated by the new dish
        int caloriesAfter = _caloriesPerDay[slot.Day]
            - _problem.Catalog.Calories[current]
            + _problem.Catalog.Calories[candidate];
        
        return caloriesAfter <= _problem.Spec.MaxDailyCalories;
    }

    // Assign a dish to an empty meal slot
    public void Place(SlotDescriptor slot, int dish)
    {
        // Similarity with neighbours is calculated before storing
        // the dish in array to not to compare it with itself
        MonotonySum += NeighbourSimilarity(slot.Index, dish);

        // If the dish was never used before adding
        // it increases the number of distinct dishes
        if (_useCount[dish] == 0)
        {
            DistinctDishCount++;
        }

        // Adds the preference penalty
        PreferenceSum += _problem.PreferencePenalty(dish);

        // Assigns the dish
        _assignment[slot.Index] = dish;
        _useCount[dish]++;
        _usedDays[dish] |= DayMask.OfDay(slot.Day);
        _caloriesPerDay[slot.Day] += _problem.Catalog.Calories[dish];
        _assignedPerDay[slot.Day]++;
        SpentMinorUnits += _problem.Catalog.CostMinorUnits[dish];
        AssignedCount++;

        // The dish is assigned and no longer considered "remaining"
        _remainingWeight -= _problem.Domains.AverageCostForKind(slot.Kind);
        _remainingMinCost -= _problem.Domains.MinCostForKind(slot.Kind);
    }

    // Removes the dish from the slot. The exact opposite of Place
    public void Remove(SlotDescriptor slot)
    {
        // Gets the current assignment
        int dish = _assignment[slot.Index];

        // Cannot remove a dish from empty slot
        if (dish == _unassigned)
        {
            return;
        }

        // Removes the dish from _assignment
        _assignment[slot.Index] = _unassigned;

        // Decreases the number of dish's usage
        _useCount[dish]--;

        // If now this dish has not appeared in the plan
        // at all the number of distinct dishes decreases
        if (_useCount[dish] == 0)
        {
            DistinctDishCount--;
        }

        // Its preference penalty is removed
        PreferenceSum -= _problem.PreferencePenalty(dish);

        // Removes the neighbour similarity after the slot is empty
        MonotonySum -= NeighbourSimilarity(slot.Index, dish);

        // Calories, dishes assigned for the day, spent minor
        // units, and number of assignet slots are also reduces
        _caloriesPerDay[slot.Day] -= _problem.Catalog.Calories[dish];
        _assignedPerDay[slot.Day]--;
        SpentMinorUnits -= _problem.Catalog.CostMinorUnits[dish];
        AssignedCount--;

        // Remaining values are restored since this slot is still "remaining"
        _remainingWeight += _problem.Domains.AverageCostForKind(slot.Kind);
        _remainingMinCost += _problem.Domains.MinCostForKind(slot.Kind);

        // Removes the current day from the bit mask of the days on which
        // the dish was used by using the bitwise NOT(~) and biwise AND(&)
        // If the gap is not 0 the dish cannot be used twice for 1 day
        // If the gap is 0 the mask is not used in checks
        _usedDays[dish] &= ~DayMask.OfDay(slot.Day);
    }

    // Replaces the dish in the slot
    public void Replace(SlotDescriptor slot, int dish)
    {
        Remove(slot);
        Place(slot, dish);
    }

    // Similarity of the dish to already assigned neighbouring slots
    private double NeighbourSimilarity(int slotIndex, int dish)
    {
        double total = 0;

        // Similarity with the previous slot
        if (slotIndex > 0 && _assignment[slotIndex - 1] != _unassigned)
        {
            total += _problem.Similarity.Between(_assignment[slotIndex - 1], dish);
        }

        // Similarity with the next slot
        if (slotIndex + 1 < _assignment.Length && _assignment[slotIndex + 1] != _unassigned)
        {
            total += _problem.Similarity.Between(dish, _assignment[slotIndex + 1]);
        }

        // Returns total similarity 
        return total;
    }
}
