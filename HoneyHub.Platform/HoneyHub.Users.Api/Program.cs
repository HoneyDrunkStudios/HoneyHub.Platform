using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using HoneyHub.Logging;
using HoneyHub.Platform.ServiceDefaults;
using HoneyHub.Users.Api.Endpoints;
using HoneyHub.Users.Api.Extensions;
using HoneyHub.Users.Api.Infrastructure;
using HoneyHub.Users.DataService.Context;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddHoneyHubSerilog(cfg =>
{
    // optional per-app tuning; safe to leave empty
    // cfg.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning);
});

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

builder.Services.AddOpenTelemetry()
    .ConfigureResource(rb =>
        rb.AddService(
            serviceName: builder.Environment.ApplicationName,
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString()
        )
        .AddAttributes(
        [
            new KeyValuePair<string, object>("deployment.environment",
                builder.Configuration["HONEYHUB_ENV"] ?? builder.Environment.EnvironmentName)
        ]))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation());

builder.Logging.AddOpenTelemetry(o =>
{
    o.IncludeScopes = true;
    o.ParseStateValues = true;
});

builder.AddDevConfigOverrides();

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddUsersDbWithLocalDev(builder.Configuration, builder.Environment);
builder.Services.AddScoped<HoneyHub.Core.DataService.Context.BaseContext>(sp => sp.GetRequiredService<UsersContext>());

builder.Services.AddUsersDataServices();
builder.Services.AddUsersAppServices(builder.Configuration);

builder.Services.AddApiValidation();

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

await app.EnsureDbConnectivityAsync();

app.MapUsersEndpoints();

await app.RunAsync();
