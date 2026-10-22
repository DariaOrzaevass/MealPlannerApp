using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Database;

// The main EF Core database context that connects the application to the database
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    // Overrides OnModelCreating method to build the database
    // model from Configurations in the MealPlanner.Infrastructure
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Calls the base Db.Context model configuration
        base.OnModelCreating(modelBuilder);
    }
}
