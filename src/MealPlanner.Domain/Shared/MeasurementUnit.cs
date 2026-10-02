using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Shared;

//Measurement unit of the ingredients in the dish
public enum MeasurementUnit
{
    Gram = 0,

    Milliliter = 1,

    Piece = 2,

    Tablespoon = 3,

    Teaspoon = 4,

    ToTaste = 5
}
