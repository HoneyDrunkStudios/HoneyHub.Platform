using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using HoneyHub.Platform.ServiceDefaults;
using HoneyHub.Users.Api.Endpoints;
using HoneyHub.Users.Api.Extensions;
using HoneyHub.Users.Api.Infrastructure;
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

// Key Vault config provider (CLI in Dev, Default in Azure)
var kvName = builder.Configuration["KeyVault:Name"];
var orgPrefix = builder.Configuration["KeyVault:OrgPrefix"] ?? "Org";
var service = builder.Configuration["KeyVault:Service"] ?? "UsersApi";
var envName = builder.Environment.EnvironmentName;

if (!string.IsNullOrWhiteSpace(kvName))
{
    TokenCredential cred = builder.Environment.IsDevelopment()
        ? new AzureCliCredential()
        : new DefaultAzureCredential();

    var kvUri = new Uri($"https://{kvName}.vault.azure.net/");
    var client = new SecretClient(kvUri, cred);
    builder.Configuration.AddAzureKeyVault(
        client,
        new DualPrefixKeyVaultSecretManager(orgPrefix, $"{service}-{envName}")
    );
}

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<UsersContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Database connection string 'DefaultConnection' is not configured. " +
            "Please ensure the connection string is set via Key Vault, appsettings, or environment variables.");
    }
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<HoneyHub.Core.DataService.Context.BaseContext>(provider =>
    provider.GetRequiredService<UsersContext>());

builder.Services.AddUsersMappings();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddApiValidation();
builder.Services.AddScoped<IUserServiceValidator, UserServiceValidator>();

builder.Services.AddScoped<IUserDataService, UserDataService>();
builder.Services.AddScoped<ISubscriptionPlanDataService, SubscriptionPlanDataService>();

builder.Services.AddPasswordServices(builder.Configuration);

// Sentry
builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration["Sentry:Dsn"];
    o.Environment = builder.Environment.EnvironmentName;
    o.TracesSampleRate = builder.Configuration.GetValue("Sentry:TracesSampleRate", 1.0);
    o.ProfilesSampleRate = builder.Configuration.GetValue("Sentry:ProfilesSampleRate", 1.0);
    o.SendDefaultPii = false;
});

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
