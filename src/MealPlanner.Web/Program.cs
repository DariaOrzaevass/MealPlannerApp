using MealPlanner.Application;
using MealPlanner.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Registers services of the Application layer
//builder.Services.AddApplication(builder.Configuration);

// Registers services of the Infrastructure layer
//builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();
