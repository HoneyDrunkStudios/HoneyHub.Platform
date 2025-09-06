using HoneyHub.Users.AppService.Models.Requests;
using HoneyHub.Users.DataService.DataServices.Subscriptions;
using Microsoft.Extensions.Logging;

namespace HoneyHub.Users.AppService.Services.Validators;

/// <summary>
/// Validation service for user-related business rules and data integrity checks.
/// Encapsulates validation logic to maintain clean separation of concerns in UserService.
/// Implements domain-specific validation rules while ensuring data consistency.
/// </summary>
/// <remarks>
/// Initializes a new instance of UserServiceValidator with required dependencies.
/// Follows Dependency Inversion Principle by depending on abstractions.
/// </remarks>
public class UserServiceValidator(
    ISubscriptionPlanDataService subscriptionPlanDataService,
    ILogger<UserServiceValidator> logger) : IUserServiceValidator
{
    private readonly ISubscriptionPlanDataService _subscriptionPlanDataService = subscriptionPlanDataService ?? throw new ArgumentNullException(nameof(subscriptionPlanDataService));
    private readonly ILogger<UserServiceValidator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Validates and retrieves subscription plan, ensuring it exists and is active.
    /// Implements business rule that users must have a valid subscription plan.
    /// </summary>
    public async Task<int> ValidateAndGetSubscriptionPlanIdAsync(int? requestedPlanId, CancellationToken cancellationToken = default)
    {
        if (requestedPlanId.HasValue)
        {
            _logger.LogDebug("Validating subscription plan ID: {SubscriptionPlanId}", requestedPlanId.Value);

            var plan = await _subscriptionPlanDataService.GetById(requestedPlanId.Value);
            if (plan is null)
            {
                _logger.LogWarning("Subscription plan with ID {SubscriptionPlanId} not found", requestedPlanId.Value);
                throw new ArgumentException($"Subscription plan with ID {requestedPlanId.Value} is not found.");
            }

            if (!plan.IsActive)
            {
                _logger.LogWarning("Subscription plan with ID {SubscriptionPlanId} is inactive", requestedPlanId.Value);
                throw new ArgumentException($"Subscription plan with ID {requestedPlanId.Value} is inactive.");
            }

            _logger.LogDebug("Subscription plan ID {SubscriptionPlanId} validated successfully", requestedPlanId.Value);
            return requestedPlanId.Value;
        }

        // Return default subscription plan ID (based on database default)
        const int defaultPlanId = 1;
        _logger.LogDebug("Using default subscription plan ID: {DefaultPlanId}", defaultPlanId);
        return defaultPlanId;
    }

    /// <summary>
    /// Validates authentication method configuration for administrative user creation.
    /// Implements business rule that admin users must have at least one authentication method.
    /// </summary>
    public void ValidateAuthenticationMethod(AdminCreateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var hasPassword = !string.IsNullOrWhiteSpace(request.Password);
        var hasExternalProvider = !string.IsNullOrWhiteSpace(request.Provider) && !string.IsNullOrWhiteSpace(request.ProviderId);

        _logger.LogDebug("Validating authentication method for admin user creation: hasPassword={HasPassword}, hasExternalProvider={HasExternalProvider}", 
            hasPassword, hasExternalProvider);

        if (!hasPassword && !hasExternalProvider)
        {
            _logger.LogWarning("Admin user creation failed: No authentication method specified");
            throw new ArgumentException("Either password or external provider (Provider + ProviderId) must be specified for admin user creation.");
        }

        if (hasExternalProvider && (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.ProviderId)))
        {
            _logger.LogWarning("Admin user creation failed: Incomplete external provider configuration");
            throw new ArgumentException("Both Provider and ProviderId must be specified for external authentication.");
        }

        _logger.LogDebug("Authentication method validation passed for admin user creation");
    }

    /// <summary>
    /// Validates password user creation request for business rule compliance.
    /// Implements validation rules specific to password-based authentication.
    /// </summary>
    public void ValidatePasswordUserRequest(CreatePasswordUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("Validating password user creation request for username: {UserName}", request.UserName);

        // Basic validation - data annotations handle most validation,
        // but we can add business-specific rules here
        if (string.IsNullOrWhiteSpace(request.UserName?.Trim()))
        {
            throw new ArgumentException("Username cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(request.Email?.Trim()))
        {
            throw new ArgumentException("Email cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password cannot be empty for password-based authentication.");
        }

        // Business rule: Username and email cannot be the same (case-insensitive)
        if (string.Equals(request.UserName.Trim(), request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Password user creation validation failed: Username and email cannot be identical");
            throw new ArgumentException("Username and email address cannot be identical.");
        }

        _logger.LogDebug("Password user creation request validation passed");
    }

    /// <summary>
    /// Validates external user creation request for business rule compliance.
    /// Implements validation rules specific to external authentication providers.
    /// </summary>
    public void ValidateExternalUserRequest(CreateExternalUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("Validating external user creation request for username: {UserName}, provider: {Provider}", 
            request.UserName, request.Provider);

        // Basic validation
        if (string.IsNullOrWhiteSpace(request.UserName?.Trim()))
        {
            throw new ArgumentException("Username cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(request.Email?.Trim()))
        {
            throw new ArgumentException("Email cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(request.Provider?.Trim()))
        {
            throw new ArgumentException("External provider name is required for external authentication.");
        }

        if (string.IsNullOrWhiteSpace(request.ProviderId?.Trim()))
        {
            throw new ArgumentException("External provider ID is required for external authentication.");
        }

        // Business rule: Validate known external providers
        var supportedProviders = new[] { "Google", "Microsoft", "GitHub", "Apple", "Facebook" };
        if (!supportedProviders.Contains(request.Provider.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Unsupported external provider: {Provider}", request.Provider);
            throw new ArgumentException($"External provider '{request.Provider}' is not supported. Supported providers: {string.Join(", ", supportedProviders)}");
        }

        // Business rule: Username and email cannot be the same (case-insensitive)
        if (string.Equals(request.UserName.Trim(), request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("External user creation validation failed: Username and email cannot be identical");
            throw new ArgumentException("Username and email address cannot be identical.");
        }

        _logger.LogDebug("External user creation request validation passed");
    }

    /// <summary>
    /// Validates administrative user creation request for business rule compliance.
    /// Performs comprehensive validation including authentication method verification.
    /// </summary>
    public void ValidateAdminUserRequest(AdminCreateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("Validating admin user creation request for username: {UserName}", request.UserName);

        // Basic validation
        if (string.IsNullOrWhiteSpace(request.UserName?.Trim()))
        {
            throw new ArgumentException("Username cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(request.Email?.Trim()))
        {
            throw new ArgumentException("Email cannot be empty or whitespace.");
        }

        // Validate authentication method
        ValidateAuthenticationMethod(request);

        // Business rule: Username and email cannot be the same (case-insensitive)
        if (string.Equals(request.UserName.Trim(), request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Admin user creation validation failed: Username and email cannot be identical");
            throw new ArgumentException("Username and email address cannot be identical.");
        }

        // Business rule: If external provider is specified, validate it's supported
        if (!string.IsNullOrWhiteSpace(request.Provider))
        {
            var supportedProviders = new[] { "Google", "Microsoft", "GitHub", "Apple", "Facebook" };
            if (!supportedProviders.Contains(request.Provider.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Unsupported external provider in admin creation: {Provider}", request.Provider);
                throw new ArgumentException($"External provider '{request.Provider}' is not supported. Supported providers: {string.Join(", ", supportedProviders)}");
            }
        }

        _logger.LogDebug("Admin user creation request validation passed");
    }
}
