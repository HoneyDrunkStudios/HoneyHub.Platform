using HoneyHub.Users.Api.Sdk.Requests;
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

            return requestedPlanId.Value;
        }

        // Return default subscription plan ID (based on database default)
        const int defaultPlanId = 1;
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

        if (!hasPassword)
        {
            _logger.LogWarning("Admin user creation failed: No authentication method specified");
            throw new ArgumentException("Password must be specified for admin user creation.");
        }
    }

    /// <summary>
    /// Validates password user creation request for business rule compliance.
    /// Implements validation rules specific to password-based authentication.
    /// </summary>
    public void ValidatePasswordUserRequest(CreatePasswordUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Basic validation - data annotations handle most validation,
        // but we can add business-specific rules here
        if (string.IsNullOrWhiteSpace(request.Username?.Trim()))
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
        if (string.Equals(request.Username.Trim(), request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Password user creation validation failed: Username and email cannot be identical");
            throw new ArgumentException("Username and email address cannot be identical.");
        }
    }

    /// <summary>
    /// Validates external user creation request for business rule compliance.
    /// Implements validation rules specific to external authentication providers.
    /// </summary>
    public void ValidateExternalUserRequest(CreateExternalUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Basic validation
        if (string.IsNullOrWhiteSpace(request.Username?.Trim()))
        {
            throw new ArgumentException("Username cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(request.Email?.Trim()))
        {
            throw new ArgumentException("Email cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(request.ProviderId?.Trim()))
        {
            throw new ArgumentException("External provider ID is required for external authentication.");
        }

        // Business rule: Username and email cannot be the same (case-insensitive)
        if (string.Equals(request.Username.Trim(), request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("External user creation validation failed: Username and email cannot be identical");
            throw new ArgumentException("Username and email address cannot be identical.");
        }
    }

    /// <summary>
    /// Validates administrative user creation request for business rule compliance.
    /// Performs comprehensive validation including authentication method verification.
    /// </summary>
    public void ValidateAdminUserRequest(AdminCreateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Basic validation
        if (string.IsNullOrWhiteSpace(request.Username?.Trim()))
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
        if (string.Equals(request.Username.Trim(), request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Admin user creation validation failed: Username and email cannot be identical");
            throw new ArgumentException("Username and email address cannot be identical.");
        }
    }
}
