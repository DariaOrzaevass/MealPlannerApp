using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Shared;

//The type of the specific meal
[Flags]
public enum MealTypes
{
    //Does not fit for any type of meal
    None = 0,

    Breakfast = 1,

    Lunch = 2,

    Dinner = 4,

    Snack = 8,

    //Will fit for any type of meal
    All = Breakfast | Lunch | Dinner | Snack
}
