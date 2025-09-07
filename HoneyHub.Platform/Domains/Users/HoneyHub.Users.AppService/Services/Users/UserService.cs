using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.AppService.Services.SecurityServices;
using HoneyHub.Users.AppService.Services.Validators;
using HoneyHub.Users.DataService.DataServices.Users;
using HoneyHub.Users.DataService.Entities.Identity;
using HoneyHub.Users.DataService.Entities.Users;
using Microsoft.Extensions.Logging;

namespace HoneyHub.Users.AppService.Services.Users;

/// <summary>
/// Application service for user management operations.
/// Orchestrates user creation workflows while maintaining domain boundaries and business rules.
/// </summary>
/// <remarks>
/// Initializes a new instance of UserService with required dependencies.
/// Follows Dependency Inversion Principle by depending on abstractions.
/// </remarks>
public class UserService(
    IUserDataService userDataService,
    IPasswordService passwordService,
    IUserServiceValidator validator,
    ILogger<UserService> logger) : IUserService
{
    private readonly IUserDataService _userDataService = userDataService ?? throw new ArgumentNullException(nameof(userDataService));
    private readonly IPasswordService _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
    private readonly IUserServiceValidator _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ILogger<UserService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Creates a new user account with password-based authentication.
    /// Implements secure password hashing and enforces business rules for password users.
    /// </summary>
    public async Task<Guid> CreatePasswordUserAsync(CreatePasswordUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate request using dedicated validator
        _validator.ValidatePasswordUserRequest(request);

        // Validate subscription plan
        var subscriptionPlanId = await _validator.ValidateAndGetSubscriptionPlanIdAsync(request.SubscriptionPlanId, cancellationToken);

        // Generate secure password hash
        var salt = _passwordService.CreateSalt();
        var passwordHash = _passwordService.HashPassword(request.Password, salt);
        var combinedHash = $"{salt}:{passwordHash}"; // Store salt and hash together for easier management

        // Create user entity with password authentication
        var user = new UserEntity
        {
            PublicId = Guid.NewGuid(),
            UserName = request.Username.Trim(),
            NormalizedUserName = request.Username.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = false, // Always require email confirmation for password users
            PasswordHash = combinedHash,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumber = null, // Not available in simplified request
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            IsActive = true,
            IsDeleted = false,
            SubscriptionPlanId = subscriptionPlanId
        };

        // Set audit information
        user.SetCreatedOn(request.Username);

        // Persist user
        await _userDataService.Insert(user);

        try
        {
            await _userDataService.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating password user with username: {Username}", request.Username);
            throw; // Rethrow to let higher layers handle it
        }

        return user.PublicId;
    }

    /// <summary>
    /// Creates a new user account with external authentication provider.
    /// Associates the user with an external login provider for authentication.
    /// </summary>
    public async Task<Guid> CreateExternalUserAsync(CreateExternalUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate request using dedicated validator
        _validator.ValidateExternalUserRequest(request);

        // Validate subscription plan
        var subscriptionPlanId = await _validator.ValidateAndGetSubscriptionPlanIdAsync(request.SubscriptionPlanId, cancellationToken);

        // Create user entity without password (external authentication)
        var user = new UserEntity
        {
            PublicId = Guid.NewGuid(),
            UserName = request.Username.Trim(),
            NormalizedUserName = request.Username.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = true, // Trust external provider verification by default
            PasswordHash = null, // No password for external users
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumber = null, // Not available in simplified request
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            IsActive = true,
            IsDeleted = false,
            SubscriptionPlanId = subscriptionPlanId
        };

        // Set audit information
        user.SetCreatedOn(request.Username);

        // Create external login association
        var userLogin = new UserLoginEntity
        {
            LoginProvider = request.Provider.ToString(),
            ProviderKey = request.ProviderId.Trim(),
            ProviderDisplayName = request.Provider.ToString(), // Use enum name as display name
            UserId = user.Id, // Will be set after user is saved
            User = user
        };

        user.Logins.Add(userLogin);

        // Persist user with external login
        await _userDataService.Insert(user);
        await _userDataService.SaveChangesAsync(cancellationToken);

        return user.PublicId;
    }

    /// <summary>
    /// Creates a new user account as an administrator with elevated configuration options.
    /// Allows setting specific flags and bypassing standard validations.
    /// </summary>
    public async Task<Guid> AdminCreateUserAsync(AdminCreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate request using dedicated validator
        _validator.ValidateAdminUserRequest(request);

        // Validate subscription plan
        var subscriptionPlanId = await _validator.ValidateAndGetSubscriptionPlanIdAsync(request.SubscriptionPlanId, cancellationToken);

        // Determine authentication configuration
        var hasPassword = !string.IsNullOrWhiteSpace(request.Password);

        // Generate password hash if password is provided
        string? passwordHash = null;
        if (hasPassword)
        {
            var salt = _passwordService.CreateSalt();
            var hash = _passwordService.HashPassword(request.Password!, salt);
            passwordHash = $"{salt}:{hash}";
        }

        // Create user entity with administrative settings
        var user = new UserEntity
        {
            PublicId = Guid.NewGuid(),
            UserName = request.Username.Trim(),
            NormalizedUserName = request.Username.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = true, // Admin created users are verified by default
            PasswordHash = passwordHash,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumber = null, // Not available in simplified request
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            IsActive = true, // Admin created users are active by default
            IsDeleted = false,
            SubscriptionPlanId = subscriptionPlanId
        };

        // Set audit information
        user.SetCreatedOn(request.Username);

        // Persist user
        await _userDataService.Insert(user);
        await _userDataService.SaveChangesAsync(cancellationToken);

        return user.PublicId;
    }
}
