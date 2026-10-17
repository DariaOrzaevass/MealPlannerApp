using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Users;

// Roles in the system
public static class Roles
{
    // A simple user of the system. Can create meal plans and store them
    public const string Member = "Member";

    // Administrator can manage dish catalog and work with ingredients
    public const string Administrator = "Administrator";
    
    public static IReadOnlyList<string> All => [Member, Administrator];
}
