using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Dishes;


// Class containing all errors related to dishes
public static class DishErrors
{
    // Returns an error if the dish with this ID is not found
    public static Error NotFound(DishId id) =>
        Error.NotFound("Dish.NotFound", $"Dish with ID {id} not found.");

    // Returns an error if the name of the dish is empty
    public static readonly Error EmptyName =
        Error.Validation("Dish.EmptyName", "Dish name cannot be empty.");

}
