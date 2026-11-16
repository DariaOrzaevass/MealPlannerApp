namespace MealPlanner.Web.Endpoints;

// Defines a common contract for API endpoint classes
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
