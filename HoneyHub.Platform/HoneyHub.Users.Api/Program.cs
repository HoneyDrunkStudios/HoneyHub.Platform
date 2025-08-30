using HoneyHub.Platform.ServiceDefaults;
using HoneyHub.Users.Api.Endpoints;
using HoneyHub.Users.AppService.Mapping;
using HoneyHub.Users.AppService.Services.Users;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register AppService layer services
builder.Services.AddUsersMappings();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map Users feature endpoints
app.MapUsersEndpoints();

app.Run();
