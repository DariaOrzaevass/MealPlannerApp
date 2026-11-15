using MealPlanner.Application;
using MealPlanner.Infrastructure;
using MealPlanner.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// Registers services of the Application layer
builder.Services.AddApplication(builder.Configuration);

// Registers services of the Infrastructure layer
builder.Services.AddInfrastructure(builder.Configuration);

// Coockie-based authentication, roles, and authorization policies.
builder.Services.AddApplicationAuthentication();

builder.Services.AddControllers();

var app = builder.Build();

// Applies migrations and fill the catalog with data
await app.PrepareDatabaseAsync();

app.UseAuthorization();

app.MapControllers();

app.Run();
