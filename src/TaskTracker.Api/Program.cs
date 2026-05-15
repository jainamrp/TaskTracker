using System.Text.Json.Serialization; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data; 
using TaskTracker.Api.Filters;
using TaskTracker.Api.Repositories.Implementations;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Services.Implementations;
using TaskTracker.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args); // Initializes the web
                                                  // application builder with command-line arguments.

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<APIExceptionFilterAttribute>(); // Adds a global exception filter.
        options.Filters.Add<ValidateModelAttribute>(); // Adds a global model validation filter.
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // Configures enums to be serialized as strings in JSON.
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true; // Disables the default model state validation to use custom validation.
});

builder.Services.AddDbContext<TaskTrackerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=tasks.db")); // Registers the EF Core DbContext with a SQLite connection string.

builder.Services.AddScoped<ITaskRepository, TaskRepository>(); // Registers the task repository for dependency injection (scoped lifetime).
builder.Services.AddScoped<ITaskService, TaskService>(); // Registers the task service for dependency injection (scoped lifetime).

var app = builder.Build(); // Builds the web application.

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskTrackerDbContext>();
    db.Database.EnsureCreated(); // Ensures the database is created at startup.
}

app.UseMiddleware<MessageHandler>(); // Adds the custom logging middleware to the pipeline.

app.MapControllers(); // Maps controller endpoints to routes.

app.Run(); // Starts the web application.

public partial class Program { } // Declares a partial Program class (useful for integration testing).