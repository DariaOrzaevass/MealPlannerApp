using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Web.Extensions;

// Turns application-level Result error into HTTP API error response
public static class ApiResults
{
    public static IResult Problem(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException("A successful result cannot be turned into error.");
        }

        return Results.Problem(
            title: result.Error.Code,
            detail: result.Error.Description,
            statusCode: StatusCode(result.Error.Type),
            extensions: Extensions(result.Error));
    }

    private static int StatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Problem => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    // Converts a composite validation error into a list.
    private static Dictionary<string, object?>? Extensions(Error error) =>
        error is ValidationError validation
            ? new Dictionary<string, object?>
            {
                ["errors"] = validation.Errors
                    .Select(e => new { code = e.Code, description = e.Description })
                    .ToArray()
            }
            : null;
}
