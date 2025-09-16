// FILE: Shared/HoneyHub.Logging/LoggingSetup.cs
namespace HoneyHub.Logging;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides opinionated Serilog setup for HoneyHub applications.
/// Consumers should call <c>builder.AddHoneyHubSerilog()</c> in Program.cs before building the host.
/// Optionally, call <c>app.UseHoneyHubRequestLogging()</c> in the HTTP pipeline to enable request logging.
/// </summary>
public static class LoggingSetup
{
    /// <summary>
    /// Configures Serilog for the given host builder using configuration-driven settings with sensible defaults.
    /// - Reads settings from the <c>Serilog</c> configuration section when present.
    /// - Adds common enrichers (environment, machine, process, thread, context, application name).
    /// - Falls back to a JSON Console sink when no sinks are configured.
    /// This overload targets <see cref="IHostApplicationBuilder"/> and wires Serilog into <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configure">Optional additional configuration for <see cref="LoggerConfiguration"/>.</param>
    /// <returns>The same <paramref name="builder"/> instance for chaining.</returns>
    public static IHostApplicationBuilder AddHoneyHubSerilog(this IHostApplicationBuilder builder, Action<LoggerConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var applicationName = builder.Environment.ApplicationName;

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", applicationName);

        // Provide a JSON Console sink when no sinks are configured in settings.
        if (!HasWriteToConfigured(builder.Configuration))
        {
            loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
        }

        configure?.Invoke(loggerConfiguration);

        Log.Logger = loggerConfiguration.CreateLogger();

        // Replace default providers and use Serilog for Microsoft.Extensions.Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger, dispose: true);

        // Ensure Serilog flushes on clean shutdown.
        builder.Services.AddHostedService(_ => new DelegatingHostedService(onStop: () => Log.CloseAndFlush()));

        return builder;
    }

    /// <summary>
    /// Enables Serilog request logging in the HTTP request pipeline with additional diagnostic context.
    /// Call this for ASP.NET Core apps (including Blazor Server) after building the app.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same <paramref name="app"/> instance for chaining.</returns>
    public static IApplicationBuilder UseHoneyHubRequestLogging(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseSerilogRequestLogging(options =>
        {
            // Enrich log events with useful request details.
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);

                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
                if (remoteIp is not null)
                {
                    diagnosticContext.Set("RemoteIp", remoteIp);
                }

                var userName = httpContext.User?.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    diagnosticContext.Set("UserName", userName);
                }

                var endpoint = httpContext.GetEndpoint();
                if (endpoint is not null && endpoint.DisplayName is not null)
                {
                    diagnosticContext.Set("EndpointName", endpoint.DisplayName);
                }

                // Correlation & distributed tracing
                var correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId);
                }

                var activity = Activity.Current;
                if (activity is not null)
                {
                    diagnosticContext.Set("TraceId", activity.TraceId.ToString());
                    diagnosticContext.Set("SpanId", activity.SpanId.ToString());
                    diagnosticContext.Set("ParentSpanId", activity.ParentSpanId.ToString());
                }
            };
        });

        return app;
    }

    private static bool HasWriteToConfigured(IConfiguration configuration)
    {
        var writeToSection = configuration.GetSection("Serilog:WriteTo");
        return writeToSection.Exists() && writeToSection.GetChildren().Any();
    }

    /// <summary>
    /// Minimal hosted service used to perform an action on host stop.
    /// Here it ensures Serilog is flushed on shutdown.
    /// </summary>
    private sealed class DelegatingHostedService(Action? onStop) : IHostedService
    {
        private readonly Action? _onStop = onStop;

        public Task StartAsync(CancellationToken _) => Task.CompletedTask;
        public Task StopAsync(CancellationToken _) { _onStop?.Invoke(); return Task.CompletedTask; }
    }
}
