using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MealPlanner.Web.Endpoints;

// Fills the OpenAPI document header iiformation
internal sealed class OpenApiDocumentInfo : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Meal Planner API",
            Version = "v1",
            Description =
                "Generation of personalized meal plans. The problem is defined" +
                "as a constrained optimization problem and is solved using" +
                "4 different algorithms: the greedy algorithm, Dijkstra's" +
                "algorithm, Backtracking, and Branch and Bound. The choice of " +
                "algorithm is a parameter of the meal-plan generation request."
        };

        return Task.CompletedTask;
    }
}
