using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Users;

namespace MealPlanner.Domain.MealPlans;

// A Meal Plan entity representing a specific Meal Plan
public sealed class MealPlan : Entity<MealPlanId>
{
    // The list of all the items in the meal plan
    private readonly List<MealPlanEntry> _entries = [];

    // Private constructor so that the dish can only be created using the Create method with validation
    private MealPlan(
    MealPlanId id,
    UserId userId,
    string name,
    DateOnly startDate,
    PlanningConstraints constraints,
    PlannerAlgorithm algorithm)
    : base(id)
    {
        UserId = userId;
        Name = name;
        StartDate = startDate;
        Constraints = constraints;
        Algorithm = algorithm;
        CreatedOnUtc = DateTime.UtcNow;
    }

    // Default constructor for EF core to get the Meal Plan from the database
    private MealPlan()
    {
    }

    // Id of the user to whom the meal plan belongs to
    public UserId UserId { get; private set; } = null!;

    // Name of the meal plan
    public string Name { get; private set; } = null!;

    // When the meal olan begins
    public DateOnly StartDate { get; private set; }

    // Constraints of the plan (budget and etc.)
    public PlanningConstraints Constraints { get; private set; } = null!;

    // The algorithm that was used for creation of this meal plan
    public PlannerAlgorithm Algorithm { get; private set; }

    // When the plan was created
    public DateTime CreatedOnUtc { get; private set; }

    // The read-only collection of entries(dishes) of the meal plan
    public IReadOnlyCollection<MealPlanEntry> Entries => _entries.AsReadOnly();
}
