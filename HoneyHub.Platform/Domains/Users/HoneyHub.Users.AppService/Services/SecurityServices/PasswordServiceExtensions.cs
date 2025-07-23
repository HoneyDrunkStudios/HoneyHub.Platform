using HoneyHub.Users.AppService.Services.SecurityServices.InternalModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HoneyHub.Users.AppService.Services.SecurityServices;

/// <summary>
/// Extension methods for configuring password services in the dependency injection container.
/// Provides a clean API for service registration with proper configuration binding and validation.
/// </summary>
public static class PasswordServiceExtensions
{
	/// <summary>
	/// Adds password services to the service collection with proper configuration binding and validation.
	/// Automatically configures PasswordHashingOptions from the "PasswordHashing" configuration section.
	/// </summary>
	/// <param name="services">The service collection to add services to</param>
	/// <param name="configuration">Configuration containing the "PasswordHashing" section</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddPasswordServices(this IServiceCollection services, IConfiguration configuration)
	{
		// Configure and validate PasswordHashingOptions from configuration
		var section = configuration.GetSection("PasswordHashing");
		services.Configure<PasswordHashingOptions>(section);
		services.AddSingleton<IValidateOptions<PasswordHashingOptions>, PasswordHashingOptionsValidator>();

		// Set development defaults if configuration section is missing
		services.PostConfigure<PasswordHashingOptions>(options =>
		{
			if (!section.Exists())
			{
				var defaults = PasswordHashingOptions.Development;
				options.DegreeOfParallelism = defaults.DegreeOfParallelism;
				options.Iterations = defaults.Iterations;
				options.MemorySize = defaults.MemorySize;
				options.HashLength = defaults.HashLength;
			}
		});

		// Register the password service as scoped (suitable for request-scoped operations)
		services.AddScoped<IPasswordService, PasswordService>();

		return services;
	}

	/// <summary>
	/// Adds password services with explicit configuration options.
	/// Useful for testing or when configuration comes from sources other than IConfiguration.
	/// </summary>
	/// <param name="services">The service collection to add services to</param>
	/// <param name="configureOptions">Action to configure the password hashing options</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddPasswordServices(this IServiceCollection services, Action<PasswordHashingOptions> configureOptions)
	{
		services.Configure(configureOptions);
		services.AddSingleton<IValidateOptions<PasswordHashingOptions>, PasswordHashingOptionsValidator>();
		services.AddScoped<IPasswordService, PasswordService>();

		return services;
	}
}