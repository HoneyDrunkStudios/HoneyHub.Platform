using HoneyHub.Users.Api.Sdk.Requests;

namespace HoneyHub.Users.AppService.Services.Validators.Users;

/// <summary>
/// Validation service interface for user-related business rules and data integrity checks.
/// Encapsulates validation logic to maintain Single Responsibility Principle in UserService.
/// </summary>
public interface IUserServiceValidator
{
    /// <summary>
    /// Validates and retrieves subscription plan, ensuring it exists and is active.
    /// Uses default subscription plan if none is specified in the request.
    /// </summary>
    /// <param name="requestedPlanId">Optional subscription plan ID from the request</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Valid subscription plan ID that can be assigned to the user</returns>
    /// <exception cref="ArgumentException">Thrown when specified plan doesn't exist or is inactive</exception>
    Task<int> ValidateAndGetSubscriptionPlanIdAsync(int? requestedPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates authentication method configuration for administrative user creation.
    /// Ensures either password or external provider is specified, but not both incorrectly.
    /// </summary>
    /// <param name="request">Administrative user creation request to validate</param>
    /// <exception cref="ArgumentException">Thrown when authentication method configuration is invalid</exception>
    void ValidateAuthenticationMethod(AdminCreateUserRequest request);

    /// <summary>
    /// Validates password user creation request for business rule compliance.
    /// Ensures all required fields are present and follow business constraints.
    /// </summary>
    /// <param name="request">Password user creation request to validate</param>
    /// <exception cref="ArgumentException">Thrown when request violates business rules</exception>
    void ValidatePasswordUserRequest(CreatePasswordUserRequest request);

    /// <summary>
    /// Validates external user creation request for business rule compliance.
    /// Ensures provider information is complete and valid.
    /// </summary>
    /// <param name="request">External user creation request to validate</param>
    /// <exception cref="ArgumentException">Thrown when request violates business rules</exception>
    void ValidateExternalUserRequest(CreateExternalUserRequest request);

    /// <summary>
    /// Validates administrative user creation request for business rule compliance.
    /// Performs comprehensive validation including authentication method verification.
    /// </summary>
    /// <param name="request">Administrative user creation request to validate</param>
    /// <exception cref="ArgumentException">Thrown when request violates business rules</exception>
    void ValidateAdminUserRequest(AdminCreateUserRequest request);
}
