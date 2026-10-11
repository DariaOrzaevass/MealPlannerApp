using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Models;

// Kinds of meal the dish fits to
[Flags]
public enum MealKinds
{
    None = 0,
    Breakfast = 1,
    Lunch = 2,
    Dinner = 4,
    Snack = 8,
    All = Breakfast | Lunch | Dinner | Snack
}
