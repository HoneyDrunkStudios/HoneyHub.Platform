using HoneyDrunk.Messaging.Domain.ValueObjects;
using HoneyHub.Outbox.Abstractions;
using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.AppService.Services.SecurityServices;
using HoneyHub.Users.AppService.Services.Validators.Users;
using HoneyHub.Users.DataService.Context;
using HoneyHub.Users.DataService.DataServices.Users;
using HoneyHub.Users.DataService.Entities.Identity;
using HoneyHub.Users.DataService.Entities.Users;
using Microsoft.Extensions.Logging;

namespace HoneyHub.Users.AppService.Services.Users;

/// <summary>
/// Application service that orchestrates user workflows and persists integration events via the transactional outbox.
/// </summary>
/// <remarks>
/// Uses a single database transaction to save domain changes and enqueue outbox records.
/// </remarks>
/// <param name="db">Users database context for transactions.</param>
/// <param name="outbox">Outbox store used to enqueue integration events.</param>
/// <param name="userDataService">Domain data service for user persistence.</param>
/// <param name="passwordService">Password hashing and salt generation service.</param>
/// <param name="validator">Validator for user creation requests and subscription plans.</param>
/// <param name="logger">Logger.</param>
public class UserService(
    UsersContext db,
    IOutboxStore outbox,
    IUserDataService userDataService,
    IPasswordService passwordService,
    IUserServiceValidator validator,
    ILogger<UserService> logger) : IUserService
{
    private readonly UsersContext _db = db ?? throw new ArgumentNullException(nameof(db));
    private readonly IOutboxStore _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
    private readonly IUserDataService _userDataService = userDataService ?? throw new ArgumentNullException(nameof(userDataService));
    private readonly IPasswordService _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
    private readonly IUserServiceValidator _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ILogger<UserService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Creates a new user with password-based authentication and enqueues a <see cref="MessageTypes.Users.UserCreated"/> outbox event.
    /// </summary>
    /// <param name="request">The request containing username, email, password, and subscription plan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The public identifier of the newly created user.</returns>
    public async Task<Guid> CreatePasswordUserAsync(CreatePasswordUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _validator.ValidatePasswordUserRequest(request);
        var subscriptionPlanId = await _validator.ValidateAndGetSubscriptionPlanIdAsync(request.SubscriptionPlanId, cancellationToken);

        if (await _userDataService.UserNameExistsAsync(request.Username.Trim().ToUpperInvariant(), cancellationToken))
            throw new ArgumentException("Username is already taken.", nameof(request));

        var salt = _passwordService.CreateSalt();
        var passwordHash = _passwordService.HashPassword(request.Password, salt);
        var combinedHash = $"{salt}:{passwordHash}";

        var user = new UserEntity
        {
            PublicId = Guid.NewGuid(),
            UserName = request.Username.Trim(),
            NormalizedUserName = request.Username.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = false,
            PasswordHash = combinedHash,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumber = null,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            IsActive = true,
            IsDeleted = false,
            SubscriptionPlanId = subscriptionPlanId
        };
        user.SetCreatedOn(request.Username);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        await _userDataService.Insert(user);

        await _outbox.EnqueueAsync(
            eventType: MessageTypes.Users.UserCreated,
            aggregateType: "User",
            aggregatePublicId: user.PublicId,
            payload: new
            {
                userId = user.PublicId,
                email = user.Email,
                username = user.UserName,
                createdAt = DateTime.UtcNow
            },
            ct: cancellationToken
        );

        try
        {
            await _userDataService.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating password user: {Username}", request.Username);
            await tx.RollbackAsync(cancellationToken);
            throw;
        }

        return user.PublicId;
    }

    /// <summary>
    /// Creates a new user associated with an external authentication provider and enqueues a <see cref="MessageTypes.Users.UserCreated"/> outbox event.
    /// </summary>
    /// <param name="request">The request containing provider details, username, email, and subscription plan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The public identifier of the newly created user.</returns>
    public async Task<Guid> CreateExternalUserAsync(CreateExternalUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _validator.ValidateExternalUserRequest(request);
        var subscriptionPlanId = await _validator.ValidateAndGetSubscriptionPlanIdAsync(request.SubscriptionPlanId, cancellationToken);

        // Ensure username uniqueness check uses normalized value for consistency
        if (await _userDataService.UserNameExistsAsync(request.Username.Trim().ToUpperInvariant(), cancellationToken))
            throw new ArgumentException("Username is already taken.", nameof(request));

        var user = new UserEntity
        {
            PublicId = Guid.NewGuid(),
            UserName = request.Username.Trim(),
            NormalizedUserName = request.Username.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = true,
            PasswordHash = null,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumber = null,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            IsActive = true,
            IsDeleted = false,
            SubscriptionPlanId = subscriptionPlanId
        };
        user.SetCreatedOn(request.Username);

        var userLogin = new UserLoginEntity
        {
            LoginProvider = request.Provider.ToString(),
            ProviderKey = request.ProviderId.Trim(),
            ProviderDisplayName = request.Provider.ToString(),
            User = user
        };
        user.Logins.Add(userLogin);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        await _userDataService.Insert(user);

        await _outbox.EnqueueAsync(
            eventType: MessageTypes.Users.UserCreated,
            aggregateType: "User",
            aggregatePublicId: user.PublicId,
            payload: new
            {
                userId = user.PublicId,
                email = user.Email,
                username = user.UserName,
                provider = request.Provider.ToString(),
                providerId = request.ProviderId,
                createdAt = DateTime.UtcNow
            },
            ct: cancellationToken
        );

        try
        {
            await _userDataService.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating external user: {Username}", request.Username);
            await tx.RollbackAsync(cancellationToken);
            throw;
        }

        return user.PublicId;
    }

    /// <summary>
    /// Creates a new user via administrative action and enqueues a <see cref="MessageTypes.Users.UserCreated"/> outbox event.
    /// </summary>
    /// <param name="request">The request containing user details, optional password, and subscription plan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The public identifier of the newly created user.</returns>
    public async Task<Guid> AdminCreateUserAsync(AdminCreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _validator.ValidateAdminUserRequest(request);
        var subscriptionPlanId = await _validator.ValidateAndGetSubscriptionPlanIdAsync(request.SubscriptionPlanId, cancellationToken);

        // Ensure username uniqueness check uses normalized value for consistency
        if (await _userDataService.UserNameExistsAsync(request.Username.Trim().ToUpperInvariant(), cancellationToken))
            throw new ArgumentException("Username is already taken.", nameof(request));

        string? passwordHash = null;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var salt = _passwordService.CreateSalt();
            var hash = _passwordService.HashPassword(request.Password!, salt);
            passwordHash = $"{salt}:{hash}";
        }

        var user = new UserEntity
        {
            PublicId = Guid.NewGuid(),
            UserName = request.Username.Trim(),
            NormalizedUserName = request.Username.Trim().ToUpperInvariant(),
            Email = request.Email.Trim(),
            NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
            EmailConfirmed = true,
            PasswordHash = passwordHash,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumber = null,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            IsActive = true,
            IsDeleted = false,
            SubscriptionPlanId = subscriptionPlanId
        };
        user.SetCreatedOn(request.Username);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        await _userDataService.Insert(user);

        await _outbox.EnqueueAsync(
            eventType: MessageTypes.Users.UserCreated,
            aggregateType: "User",
            aggregatePublicId: user.PublicId,
            payload: new
            {
                userId = user.PublicId,
                email = user.Email,
                username = user.UserName,
                hasPassword = passwordHash is not null,
                createdAt = DateTime.UtcNow
            },
            ct: cancellationToken
        );

        try
        {
            await _userDataService.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error provisioning user (admin): {Username}", request.Username);
            await tx.RollbackAsync(cancellationToken);
            throw;
        }

        return user.PublicId;
    }
}
