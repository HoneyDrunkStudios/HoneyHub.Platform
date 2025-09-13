using HoneyHub.Users.DataService.Context;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.Api.Extensions;

public static class LocalDbExtensions
{
    public static void AddDevConfigOverrides(this WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment()) return;
        builder.Configuration
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        // Optional: .AddUserSecrets<Program>(optional: true);
    }

    public static void AddUsersDbWithLocalDev(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
    {
        services.AddDbContext<UsersContext>(options =>
        {
            var cs = cfg.GetConnectionString("DefaultConnection")
                     ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            var runningInContainer = string.Equals(
                Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                "true",
                StringComparison.OrdinalIgnoreCase);

            if (env.IsDevelopment() && runningInContainer)
                cs = cs.Replace("localhost", "host.docker.internal", StringComparison.OrdinalIgnoreCase);

            options.UseSqlServer(cs);
        });

        services.AddScoped<Core.DataService.Context.BaseContext>(sp =>
            sp.GetRequiredService<UsersContext>());
    }

    public static async Task EnsureDbConnectivityAsync(this WebApplication app, TimeSpan? timeout = null)
    {
        if (!app.Environment.IsDevelopment()) return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

        var until = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(20));

        while (true)
        {
            try
            {
                await db.Database.ExecuteSqlRawAsync("SELECT 1");
                logger.LogInformation("DB connectivity OK.");
                break;
            }
            catch (Exception ex)
            {
                if (DateTime.UtcNow >= until)
                    throw new InvalidOperationException("DB not reachable.", ex);

                logger.LogWarning("Waiting for DB... {Message}", ex.Message);
                await Task.Delay(1000);
            }
        }
    }
}
