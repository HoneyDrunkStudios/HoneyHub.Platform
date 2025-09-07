using FluentValidation;
using HoneyHub.Users.Api.Validation.Users;

namespace HoneyHub.Users.Api.Extensions;

/// <summary>
/// Extension methods for configuring FluentValidation in the API project.
/// Provides centralized registration of all request validators at the API boundary.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Adds FluentValidation services and registers all request validators for the Users API.
    /// Automatically scans and registers validators in the API assembly.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddApiValidation(this IServiceCollection services)
    {
        // Add FluentValidation services and scan current assembly for validators
        services.AddValidatorsFromAssemblyContaining<CreatePasswordUserRequestValidator>();

        // Configure validation behavior globally
        ValidatorOptions.Global.LanguageManager.Enabled = false; // Use default English messages

        return services;
    }
}
