using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MealPlanner.Web.Endpoints;

// Customizes OpenApi schema for enums so that API documentation
// shows meaning of the enum value and not just numeric enum value
internal sealed class OpenApiEnumValues : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type)
            ?? context.JsonTypeInfo.Type;

        if (!type.IsEnum)
        {
            return Task.CompletedTask;
        }

        OpenApiSchema target = schema;

        string[] names = Enum.GetNames(type);
        Array values = Enum.GetValues(type);

        target.Enum = [.. values.Cast<object>().Select(v => JsonValue.Create(Convert.ToInt32(v, null)))];

        string legend = string.Join(
            ", ",
            names.Zip(values.Cast<object>(), (name, value) => $"{Convert.ToInt32(value, null)} - {name}"));

        target.Description = string.IsNullOrEmpty(target.Description)
            ? legend
            : $"{target.Description} ({legend})";

        return Task.CompletedTask;
    }
}
