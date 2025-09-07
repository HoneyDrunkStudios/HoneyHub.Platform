using HoneyHub.Platform.ServiceDefaults;
using HoneyHub.Users.Api.Endpoints;
using HoneyHub.Users.Api.Extensions;
using HoneyHub.Users.AppService.Mapping;
using HoneyHub.Users.AppService.Services.SecurityServices;
using HoneyHub.Users.AppService.Services.Users;
using HoneyHub.Users.AppService.Services.Validators.Users;
using HoneyHub.Users.DataService.Context;
using HoneyHub.Users.DataService.DataServices.Subscriptions;
using HoneyHub.Users.DataService.DataServices.Users;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// NEW: add health checks service
builder.Services.AddHealthChecks();

// Database context
builder.Services.AddDbContext<UsersContext>(options =>
{
    // Use connection string from configuration - throw error if missing
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Database connection string 'DefaultConnection' is not configured. " +
            "Please ensure the connection string is set in appsettings.json, environment variables, or other configuration sources.");
    }

    options.UseSqlServer(connectionString);
});

// Register BaseContext for data services
builder.Services.AddScoped<HoneyHub.Core.DataService.Context.BaseContext>(provider =>
    provider.GetRequiredService<UsersContext>());

// User domain services
builder.Services.AddUsersMappings();
builder.Services.AddScoped<IUserService, UserService>();

// Validation services
builder.Services.AddApiValidation(); // FluentValidation for API requests
builder.Services.AddScoped<IUserServiceValidator, UserServiceValidator>(); // Business rules

// Data services
builder.Services.AddScoped<IUserDataService, UserDataService>();
builder.Services.AddScoped<ISubscriptionPlanDataService, SubscriptionPlanDataService>();

// Security services
builder.Services.AddPasswordServices(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", o =>
         o.WithTitle("HoneyHub API").WithDarkMode(true));
}

var runningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);

if (!runningInContainer)
{
    app.UseHttpsRedirection();
}

app.MapUsersEndpoints();

app.Run();
