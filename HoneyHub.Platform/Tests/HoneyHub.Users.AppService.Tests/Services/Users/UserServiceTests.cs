using HoneyHub.Users.AppService.Models.Requests;
using HoneyHub.Users.AppService.Services.SecurityServices;
using HoneyHub.Users.AppService.Services.Users;
using HoneyHub.Users.AppService.Services.Validators;
using HoneyHub.Users.DataService.DataServices.Users;
using HoneyHub.Users.DataService.Entities.Users;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HoneyHub.Users.AppService.Tests.Services.Users;

/// <summary>
/// Unit tests for UserService following DDD principles and testing best practices.
/// Tests focus on business logic validation, domain rule enforcement, and security aspects.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserDataService> _mockUserDataService;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IUserServiceValidator> _mockValidator;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserDataService = new Mock<IUserDataService>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockValidator = new Mock<IUserServiceValidator>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _userService = new UserService(
            _mockUserDataService.Object,
            _mockPasswordService.Object,
            _mockValidator.Object,
            _mockLogger.Object);
    }

    [Fact(DisplayName = "Should create password user with valid request")]
    public async Task CreatePasswordUserAsync_ValidRequest_ReturnsUserPublicId()
    {
        // Setup
        var request = new CreatePasswordUserRequest
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "securepassword123",
            CreatedBy = "system"
        };

        var testSalt = "testSalt123";
        var testHash = "testHash456";
        const int validSubscriptionPlanId = 1;

        _mockValidator.Setup(x => x.ValidatePasswordUserRequest(request));
        _mockValidator.Setup(x => x.ValidateAndGetSubscriptionPlanIdAsync(null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(validSubscriptionPlanId);
        _mockPasswordService.Setup(x => x.CreateSalt()).Returns(testSalt);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password, testSalt)).Returns(testHash);
        _mockUserDataService.Setup(x => x.Insert(It.IsAny<UserEntity>())).Returns(Task.CompletedTask);
        _mockUserDataService.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _userService.CreatePasswordUserAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _mockValidator.Verify(x => x.ValidatePasswordUserRequest(request), Times.Once);
        _mockValidator.Verify(x => x.ValidateAndGetSubscriptionPlanIdAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        _mockPasswordService.Verify(x => x.CreateSalt(), Times.Once);
        _mockPasswordService.Verify(x => x.HashPassword(request.Password, testSalt), Times.Once);
        _mockUserDataService.Verify(x => x.Insert(It.Is<UserEntity>(u => 
            u.UserName == "testuser" && 
            u.Email == "test@example.com" && 
            u.PasswordHash == $"{testSalt}:{testHash}" &&
            !u.EmailConfirmed)), Times.Once);
        _mockUserDataService.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Should create external user with valid request")]
    public async Task CreateExternalUserAsync_ValidRequest_ReturnsUserPublicId()
    {
        // Setup
        var request = new CreateExternalUserRequest
        {
            UserName = "externaluser",
            Email = "external@example.com",
            Provider = "Google",
            ProviderId = "google123",
            ProviderDisplayName = "Google Account",
            EmailConfirmed = true,
            CreatedBy = "system"
        };

        const int validSubscriptionPlanId = 1;

        _mockValidator.Setup(x => x.ValidateExternalUserRequest(request));
        _mockValidator.Setup(x => x.ValidateAndGetSubscriptionPlanIdAsync(null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(validSubscriptionPlanId);
        _mockUserDataService.Setup(x => x.Insert(It.IsAny<UserEntity>())).Returns(Task.CompletedTask);
        _mockUserDataService.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _userService.CreateExternalUserAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _mockValidator.Verify(x => x.ValidateExternalUserRequest(request), Times.Once);
        _mockValidator.Verify(x => x.ValidateAndGetSubscriptionPlanIdAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserDataService.Verify(x => x.Insert(It.Is<UserEntity>(u => 
            u.UserName == "externaluser" && 
            u.Email == "external@example.com" && 
            u.PasswordHash == null &&
            u.EmailConfirmed &&
            u.Logins.Any(l => l.LoginProvider == "Google" && l.ProviderKey == "google123"))), Times.Once);
        _mockUserDataService.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Should create admin user with password when valid request")]
    public async Task AdminCreateUserAsync_ValidPasswordRequest_ReturnsUserPublicId()
    {
        // Setup
        var request = new AdminCreateUserRequest
        {
            UserName = "adminuser",
            Email = "admin@example.com",
            Password = "adminpassword123",
            EmailConfirmed = true,
            IsActive = true,
            TwoFactorEnabled = true,
            CreatedBy = "administrator"
        };

        var testSalt = "adminSalt123";
        var testHash = "adminHash456";
        const int validSubscriptionPlanId = 1;

        _mockValidator.Setup(x => x.ValidateAdminUserRequest(request));
        _mockValidator.Setup(x => x.ValidateAndGetSubscriptionPlanIdAsync(null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(validSubscriptionPlanId);
        _mockPasswordService.Setup(x => x.CreateSalt()).Returns(testSalt);
        _mockPasswordService.Setup(x => x.HashPassword(request.Password, testSalt)).Returns(testHash);
        _mockUserDataService.Setup(x => x.Insert(It.IsAny<UserEntity>())).Returns(Task.CompletedTask);
        _mockUserDataService.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _userService.AdminCreateUserAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _mockValidator.Verify(x => x.ValidateAdminUserRequest(request), Times.Once);
        _mockValidator.Verify(x => x.ValidateAndGetSubscriptionPlanIdAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserDataService.Verify(x => x.Insert(It.Is<UserEntity>(u => 
            u.UserName == "adminuser" && 
            u.Email == "admin@example.com" && 
            u.PasswordHash == $"{testSalt}:{testHash}" &&
            u.EmailConfirmed &&
            u.TwoFactorEnabled)), Times.Once);
        _mockUserDataService.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Should delegate validation to validator service")]
    public async Task CreatePasswordUserAsync_ValidationFailure_PropagatesValidatorException()
    {
        // Setup
        var request = new CreatePasswordUserRequest
        {
            UserName = "testuser",
            Email = "testuser", // Invalid - same as username, validator should catch this
            Password = "securepassword123",
            CreatedBy = "system"
        };

        _mockValidator.Setup(x => x.ValidatePasswordUserRequest(request))
                     .Throws(new ArgumentException("Username and email address cannot be identical."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.CreatePasswordUserAsync(request));
        
        Assert.Contains("Username and email address cannot be identical", exception.Message);
        _mockValidator.Verify(x => x.ValidatePasswordUserRequest(request), Times.Once);
        
        // Ensure no further processing occurred
        _mockPasswordService.Verify(x => x.CreateSalt(), Times.Never);
        _mockUserDataService.Verify(x => x.Insert(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact(DisplayName = "Should delegate subscription plan validation to validator service")]
    public async Task CreatePasswordUserAsync_InvalidSubscriptionPlan_PropagatesValidatorException()
    {
        // Setup
        var request = new CreatePasswordUserRequest
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "securepassword123",
            SubscriptionPlanId = 999, // Invalid plan ID
            CreatedBy = "system"
        };

        _mockValidator.Setup(x => x.ValidatePasswordUserRequest(request));
        _mockValidator.Setup(x => x.ValidateAndGetSubscriptionPlanIdAsync(999, It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new ArgumentException("Subscription plan with ID 999 is not found."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _userService.CreatePasswordUserAsync(request));
        
        Assert.Contains("Subscription plan with ID 999 is not found", exception.Message);
        _mockValidator.Verify(x => x.ValidateAndGetSubscriptionPlanIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        
        // Ensure no user creation occurred
        _mockUserDataService.Verify(x => x.Insert(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact(DisplayName = "Should throw when request is null")]
    public async Task CreatePasswordUserAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _userService.CreatePasswordUserAsync(null!));
    }

    [Fact(DisplayName = "Should create admin user with external authentication")]
    public async Task AdminCreateUserAsync_ExternalAuthentication_ReturnsUserPublicId()
    {
        // Setup
        var request = new AdminCreateUserRequest
        {
            UserName = "adminuser",
            Email = "admin@example.com",
            Provider = "Microsoft",
            ProviderId = "microsoft123",
            ProviderDisplayName = "Microsoft Account",
            EmailConfirmed = true,
            IsActive = true,
            CreatedBy = "administrator"
        };

        const int validSubscriptionPlanId = 1;

        _mockValidator.Setup(x => x.ValidateAdminUserRequest(request));
        _mockValidator.Setup(x => x.ValidateAndGetSubscriptionPlanIdAsync(null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(validSubscriptionPlanId);
        _mockUserDataService.Setup(x => x.Insert(It.IsAny<UserEntity>())).Returns(Task.CompletedTask);
        _mockUserDataService.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _userService.AdminCreateUserAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        _mockValidator.Verify(x => x.ValidateAdminUserRequest(request), Times.Once);
        _mockUserDataService.Verify(x => x.Insert(It.Is<UserEntity>(u => 
            u.UserName == "adminuser" && 
            u.Email == "admin@example.com" && 
            u.PasswordHash == null &&
            u.EmailConfirmed &&
            u.Logins.Any(l => l.LoginProvider == "Microsoft" && l.ProviderKey == "microsoft123"))), Times.Once);
        _mockUserDataService.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Should not create password hash for external authentication
        _mockPasswordService.Verify(x => x.CreateSalt(), Times.Never);
        _mockPasswordService.Verify(x => x.HashPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
