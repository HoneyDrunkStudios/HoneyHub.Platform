using HoneyHub.Platform.ServiceDefaults;
using HoneyHub.Users.Api.Endpoints;
using HoneyHub.Users.AppService.Mapping;
using HoneyHub.Users.AppService.Services.Users;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// NEW: add health checks service
builder.Services.AddHealthChecks();

builder.Services.AddUsersMappings();
builder.Services.AddScoped<IUserService, UserService>();

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
