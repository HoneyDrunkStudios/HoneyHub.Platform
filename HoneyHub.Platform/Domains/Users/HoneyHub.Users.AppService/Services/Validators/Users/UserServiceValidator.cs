using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.DataService.DataServices.Subscriptions;
using Microsoft.Extensions.Logging;

namespace HoneyHub.Users.AppService.Services.Validators.Users;

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
    /// Validates basic user identity fields (username and email).
    /// Implements fundamental data integrity requirements for user creation.
    /// </summary>
    private static void ValidateBasicUserFields(string? username, string? email)
    {
        if (string.IsNullOrWhiteSpace(username?.Trim()))
            throw new ArgumentException("Username cannot be empty or whitespace.");

        if (string.IsNullOrWhiteSpace(email?.Trim()))
            throw new ArgumentException("Email cannot be empty or whitespace.");
    }

    /// <summary>
    /// Validates password requirement for authentication methods that require passwords.
    /// Implements data integrity rule for password-based authentication.
    /// </summary>
    private static void ValidatePasswordRequirement(string? password, string authType)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException($"Password cannot be empty for {authType} authentication.");
    }

    /// <summary>
    /// Validates provider ID requirement for external authentication.
    /// Implements data integrity rule for external authentication providers.
    /// </summary>
    private static void ValidateProviderIdRequirement(string? providerId)
    {
        if (string.IsNullOrWhiteSpace(providerId?.Trim()))
            throw new ArgumentException("External provider ID is required for external authentication.");
    }

    /// <summary>
    /// Validates that username and email are not identical (case-insensitive).
    /// Implements core business rule for user identity uniqueness.
    /// </summary>
    private void ValidateUsernameEmailUniqueness(string username, string email, string validationType)
    {
        if (string.Equals(username.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("{ValidationType} user creation validation failed: Username and email cannot be identical", validationType);
            throw new ArgumentException("Username and email address cannot be identical.");
        }
    }

    /// <summary>
    /// Validates password user creation request for business rule compliance.
    /// Implements validation rules specific to password-based authentication.
    /// </summary>
    public void ValidatePasswordUserRequest(CreatePasswordUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Basic field validation
        ValidateBasicUserFields(request.Username, request.Email);

        // Password requirement validation
        ValidatePasswordRequirement(request.Password, "password-based");

        // Business rule: Username and email cannot be the same (case-insensitive)
        ValidateUsernameEmailUniqueness(request.Username, request.Email, "Password");
    }

    /// <summary>
    /// Validates external user creation request for business rule compliance.
    /// Implements validation rules specific to external authentication providers.
    /// </summary>
    public void ValidateExternalUserRequest(CreateExternalUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Basic field validation
        ValidateBasicUserFields(request.Username, request.Email);

        // Provider ID requirement validation
        ValidateProviderIdRequirement(request.ProviderId);

        // Business rule: Username and email cannot be the same (case-insensitive)
        ValidateUsernameEmailUniqueness(request.Username, request.Email, "External");
    }

    /// <summary>
    /// Validates administrative user creation request for business rule compliance.
    /// Performs comprehensive validation including authentication method verification.
    /// </summary>
    public void ValidateAdminUserRequest(AdminCreateUserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Basic field validation
        ValidateBasicUserFields(request.Username, request.Email);

        // Validate authentication method
        ValidateAuthenticationMethod(request);

        // Business rule: Username and email cannot be the same (case-insensitive)
        ValidateUsernameEmailUniqueness(request.Username, request.Email, "Admin");
    }
}
