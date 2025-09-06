using HoneyHub.Users.AppService.Models.Requests;
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

        _logger.LogInformation("Creating password user with username: {UserName}, email: {Email}", 
            request.UserName, request.Email);

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
            UserName = request.UserName.Trim(),
            NormalizedUserName = request.UserName.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = false, // Always require email confirmation for password users
            PasswordHash = combinedHash,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            IsActive = true,
            IsDeleted = false,
            SubscriptionPlanId = subscriptionPlanId
        };

        // Set audit information
        user.SetCreatedOn(request.CreatedBy);

        // Persist user
        await _userDataService.Insert(user);

        try
        {
            await _userDataService.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating password user with username: {UserName}", request.UserName);
            throw; // Rethrow to let higher layers handle it
        }

        _logger.LogInformation("Successfully created password user with PublicId: {PublicId}", user.PublicId);

        return user.PublicId;
    }

    /// <summary>
    /// Creates a new user account with external authentication provider.
    /// Associates the user with an external login provider for authentication.
    /// </summary>
    public async Task<Guid> CreateExternalUserAsync(CreateExternalUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Creating external user with username: {UserName}, email: {Email}, provider: {Provider}", 
            request.UserName, request.Email, request.Provider);

        // Validate request using dedicated validator
        _validator.ValidateExternalUserRequest(request);

        // Validate subscription plan
        var subscriptionPlanId = await _validator.ValidateAndGetSubscriptionPlanIdAsync(request.SubscriptionPlanId, cancellationToken);

        // Create user entity without password (external authentication)
        var user = new UserEntity
        {
            PublicId = Guid.NewGuid(),
            UserName = request.UserName.Trim(),
            NormalizedUserName = request.UserName.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = request.EmailConfirmed, // Trust external provider verification
            PasswordHash = null, // No password for external users
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            IsActive = true,
            IsDeleted = false,
            SubscriptionPlanId = subscriptionPlanId
        };

        // Set audit information
        user.SetCreatedOn(request.CreatedBy);

        // Create external login association
        var userLogin = new UserLoginEntity
        {
            LoginProvider = request.Provider.Trim(),
            ProviderKey = request.ProviderId.Trim(),
            ProviderDisplayName = request.ProviderDisplayName?.Trim(),
            UserId = user.Id, // Will be set after user is saved
            User = user
        };

        user.Logins.Add(userLogin);

        // Persist user with external login
        await _userDataService.Insert(user);
        await _userDataService.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created external user with PublicId: {PublicId}, Provider: {Provider}", 
            user.PublicId, request.Provider);

        return user.PublicId;
    }

    /// <summary>
    /// Creates a new user account as an administrator with elevated configuration options.
    /// Allows setting specific flags and bypassing standard validations.
    /// </summary>
    public async Task<Guid> AdminCreateUserAsync(AdminCreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation("Admin creating user with username: {UserName}, email: {Email}, createdBy: {CreatedBy}", 
            request.UserName, request.Email, request.CreatedBy);

        // Validate request using dedicated validator
        _validator.ValidateAdminUserRequest(request);

        // Validate subscription plan
        var subscriptionPlanId = await _validator.ValidateAndGetSubscriptionPlanIdAsync(request.SubscriptionPlanId, cancellationToken);

        // Determine authentication configuration
        var hasPassword = !string.IsNullOrWhiteSpace(request.Password);
        var hasExternalProvider = !string.IsNullOrWhiteSpace(request.Provider) && !string.IsNullOrWhiteSpace(request.ProviderId);

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
            UserName = request.UserName.Trim(),
            NormalizedUserName = request.UserName.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = request.EmailConfirmed,
            PasswordHash = passwordHash,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            PhoneNumberConfirmed = request.PhoneNumberConfirmed,
            TwoFactorEnabled = request.TwoFactorEnabled,
            LockoutEnabled = request.LockoutEnabled,
            AccessFailedCount = 0,
            IsActive = request.IsActive,
            IsDeleted = false,
            SubscriptionPlanId = subscriptionPlanId
        };

        // Set audit information
        user.SetCreatedOn(request.CreatedBy);

        // Add external login if specified
        if (hasExternalProvider)
        {
            var userLogin = new UserLoginEntity
            {
                LoginProvider = request.Provider!.Trim(),
                ProviderKey = request.ProviderId!.Trim(),
                ProviderDisplayName = request.ProviderDisplayName?.Trim(),
                UserId = user.Id,
                User = user
            };

            user.Logins.Add(userLogin);
        }

        // Persist user
        await _userDataService.Insert(user);
        await _userDataService.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully admin created user with PublicId: {PublicId}, hasPassword: {HasPassword}, hasExternalProvider: {HasExternalProvider}", 
            user.PublicId, hasPassword, hasExternalProvider);

        return user.PublicId;
    }

    /// <summary>
    /// Legacy method - kept for backward compatibility.
    /// </summary>
    [Obsolete("Use CreatePasswordUserAsync, CreateExternalUserAsync, or AdminCreateUserAsync instead")]
    public Task CreateUser() => Task.CompletedTask;
}
