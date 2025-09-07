using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.DataService.DataServices.Subscriptions;
using Microsoft.Extensions.Logging;

namespace HoneyHub.Users.AppService.Services.Validators.Users;

/// <summary>
/// Business rule validation service for user-related domain logic and data integrity checks.
/// Focuses on domain-specific validation rules while technical validation is handled by FluentValidation.
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
    /// Note: Basic field validation is now handled by FluentValidation.
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
    /// Focuses on domain-specific rules as technical validation is handled by FluentValidation.
    /// </summary>
    public void ValidatePasswordUserRequest(CreatePasswordUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Domain-specific business rules can be added here
        // Basic field validation is now handled by FluentValidation

        // Example: Additional business rules could include:
        // - Username uniqueness checks (if not handled at database level)
        // - Domain-specific password policies beyond basic technical requirements
        // - Business-specific username patterns or restrictions
    }

    /// <summary>
    /// Validates external user creation request for business rule compliance.
    /// Focuses on domain-specific rules as technical validation is handled by FluentValidation.
    /// </summary>
    public void ValidateExternalUserRequest(CreateExternalUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Domain-specific business rules can be added here
        // Basic field validation is now handled by FluentValidation

        // Example: Additional business rules could include:
        // - Provider-specific validation logic
        // - Domain-specific restrictions on external providers
        // - Business-specific external user policies
    }

    /// <summary>
    /// Validates administrative user creation request for business rule compliance.
    /// Performs domain-specific validation including authentication method verification.
    /// </summary>
    public void ValidateAdminUserRequest(AdminCreateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate authentication method (domain business rule)
        ValidateAuthenticationMethod(request);

        // Domain-specific business rules can be added here
        // Basic field validation is now handled by FluentValidation

        // Example: Additional business rules could include:
        // - Admin-specific user creation policies
        // - Role-based restrictions on admin user creation
        // - Audit requirements for admin-created users
    }
}
