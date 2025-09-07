using HoneyHub.Users.Api.Sdk.Requests;

namespace HoneyHub.Users.AppService.Services.Users;

/// <summary>
/// Application service interface for user management operations.
/// Provides methods for creating users with different authentication methods.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user account with password-based authentication.
    /// Validates input, generates secure password hash, and persists user data.
    /// </summary>
    /// <param name="request">Request containing user details for password-based authentication</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The public ID of the created user</returns>
    Task<Guid> CreatePasswordUserAsync(CreatePasswordUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user account with external authentication provider.
    /// Validates provider information and creates user with external login association.
    /// </summary>
    /// <param name="request">Request containing user details for external provider authentication</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The public ID of the created user</returns>
    Task<Guid> CreateExternalUserAsync(CreateExternalUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user account as an administrator with additional configuration options.
    /// Allows administrators to set specific flags and bypass certain validations.
    /// </summary>
    /// <param name="request">Request containing user details for administrative creation</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The public ID of the created user</returns>
    Task<Guid> AdminCreateUserAsync(AdminCreateUserRequest request, CancellationToken cancellationToken = default);
}
