using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MealPlanner.Domain.Abstractions;

// Result of the operation, can be success or error
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            // If the result is success, but the error is not None(no error), the exception must be thrown
            case true when error != Error.None:
                throw new InvalidOperationException("Successful result cannot have an error.");
            // If the result is error with no returning specific error, the exception must be thrown
            case false when error == Error.None:
                throw new InvalidOperationException("Failed result must have an error.");
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    // Success if the value is not null, otherwise NullValue error
    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

// Returns result of the operation(success/failure) and some value
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    // Value of successful result
    // Will not let to read the value if the result is failure, will throw exception
    [NotNull]
    public TValue Value => IsSuccess
    ? _value!
    : throw new InvalidOperationException("Cannot read value of the failed result.");

    // Implicit conversion to make it easier to return value
    public static implicit operator Result<TValue>(TValue? value) => Create(value);
}
