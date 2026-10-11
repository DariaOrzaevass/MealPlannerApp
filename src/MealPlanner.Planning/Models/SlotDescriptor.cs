using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Models;

// The slot of the meal plan
public readonly record struct SlotDescriptor(int Index, int Day, MealKinds Kind);
