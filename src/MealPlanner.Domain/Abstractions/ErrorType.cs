using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Abstractions;

//Types of errors
public enum ErrorType
{
    //Generic error. Will turn into 500
    Failure = 0,

    //Validation error. Will turn into 400
    Validation = 1,

    //Correct request, but cannot complete it. Will turn into 400
    Problem = 2,

    //Resource not found. Will turn into 404
    NotFound = 3,

    //Conflict with current state of recource. Will turn into 409
    Conflict = 4,
}
