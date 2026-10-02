using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Shared;

//The diet type of the dish
public enum DietType
{
    //Can eat anything
    Omnivore = 0,

    //Excludes meat and fish
    Vegetarian = 1,

    //Excludes all animal products
    Vegan = 2
}
