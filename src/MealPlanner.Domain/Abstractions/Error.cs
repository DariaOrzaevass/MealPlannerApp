using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Abstractions;


// Domain error record
public record Error(string Code, string Description, ErrorType Type)
{
    // To show that there has been no error
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    // Default error with null values
    public static readonly Error NullValue =
    new("General.Null", "Empty value.", ErrorType.Failure);

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}
