using HoneyHub.Users.AppService.Models.Requests;
using HoneyHub.Users.AppService.Services.Validators;
using HoneyHub.Users.DataService.DataServices.Subscriptions;
using HoneyHub.Users.DataService.Entities.Subscriptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HoneyHub.Users.AppService.Tests.Services.Validators;

/// <summary>
/// Unit tests for UserServiceValidator following DDD principles and testing best practices.
/// Tests focus on validation logic, business rule enforcement, and error scenarios.
/// </summary>
public class UserServiceValidatorTests
{
    private readonly Mock<ISubscriptionPlanDataService> _mockSubscriptionPlanDataService;
    private readonly Mock<ILogger<UserServiceValidator>> _mockLogger;
    private readonly UserServiceValidator _validator;

    public UserServiceValidatorTests()
    {
        _mockSubscriptionPlanDataService = new Mock<ISubscriptionPlanDataService>();
        _mockLogger = new Mock<ILogger<UserServiceValidator>>();

        _validator = new UserServiceValidator(
            _mockSubscriptionPlanDataService.Object,
            _mockLogger.Object);
    }

    #region ValidateAndGetSubscriptionPlanIdAsync Tests

    [Fact(DisplayName = "Should return valid plan ID when plan exists and is active")]
    public async Task ValidateAndGetSubscriptionPlanIdAsync_ValidActivePlan_ReturnsValidPlanId()
    {
        // Setup
        const int requestedPlanId = 5;
        var activePlan = new SubscriptionPlanEntity 
        { 
            Id = requestedPlanId, 
            IsActive = true, 
            Name = "Premium Plan",
            DisplayName = "Premium Plan",
            Price = 29.99m,
            Currency = "USD",
            BillingCycle = "Monthly",
            BillingIntervalMonths = 1,
            IsPublic = true,
            IsDefault = false,
            SortOrder = 1,
            PopularBadge = false,
            Version = 1,
            EffectiveFrom = DateTime.UtcNow.AddDays(-30),
            Users = []
        };

        _mockSubscriptionPlanDataService
            .Setup(x => x.GetById(requestedPlanId))
            .ReturnsAsync(activePlan);

        // Act
        var result = await _validator.ValidateAndGetSubscriptionPlanIdAsync(requestedPlanId);

        // Assert
        Assert.Equal(requestedPlanId, result);
        _mockSubscriptionPlanDataService.Verify(x => x.GetById(requestedPlanId), Times.Once);
    }

    [Fact(DisplayName = "Should return default plan ID when no plan specified")]
    public async Task ValidateAndGetSubscriptionPlanIdAsync_NoPlanSpecified_ReturnsDefaultPlanId()
    {
        // Act
        var result = await _validator.ValidateAndGetSubscriptionPlanIdAsync(null);

        // Assert
        Assert.Equal(1, result); // Default plan ID
        _mockSubscriptionPlanDataService.Verify(x => x.GetById(It.IsAny<int>()), Times.Never);
    }

    [Fact(DisplayName = "Should throw when plan does not exist")]
    public async Task ValidateAndGetSubscriptionPlanIdAsync_PlanNotFound_ThrowsArgumentException()
    {
        // Setup
        const int nonExistentPlanId = 999;
        _mockSubscriptionPlanDataService
            .Setup(x => x.GetById(nonExistentPlanId))
            .ReturnsAsync((SubscriptionPlanEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _validator.ValidateAndGetSubscriptionPlanIdAsync(nonExistentPlanId));
        
        Assert.Contains("is not found", exception.Message);
        Assert.Contains(nonExistentPlanId.ToString(), exception.Message);
    }

    [Fact(DisplayName = "Should throw when plan exists but is inactive")]
    public async Task ValidateAndGetSubscriptionPlanIdAsync_InactivePlan_ThrowsArgumentException()
    {
        // Setup
        const int inactivePlanId = 3;
        var inactivePlan = new SubscriptionPlanEntity 
        { 
            Id = inactivePlanId, 
            IsActive = false,
            Name = "Inactive Plan",
            DisplayName = "Inactive Plan",
            Price = 19.99m,
            Currency = "USD",
            BillingCycle = "Monthly",
            BillingIntervalMonths = 1,
            IsPublic = false,
            IsDefault = false,
            SortOrder = 1,
            PopularBadge = false,
            Version = 1,
            EffectiveFrom = DateTime.UtcNow.AddDays(-30),
            Users = []
        };

        _mockSubscriptionPlanDataService
            .Setup(x => x.GetById(inactivePlanId))
            .ReturnsAsync(inactivePlan);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _validator.ValidateAndGetSubscriptionPlanIdAsync(inactivePlanId));
        
        Assert.Contains("is inactive", exception.Message);
        Assert.Contains(inactivePlanId.ToString(), exception.Message);
    }

    #endregion

    #region ValidatePasswordUserRequest Tests

    [Fact(DisplayName = "Should validate successfully with valid password user request")]
    public void ValidatePasswordUserRequest_ValidRequest_NoExceptionThrown()
    {
        // Setup
        var request = new CreatePasswordUserRequest
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "securepassword123",
            CreatedBy = "system"
        };

        // Act & Assert - Should not throw
        _validator.ValidatePasswordUserRequest(request);
    }

    [Fact(DisplayName = "Should throw when username and email are identical")]
    public void ValidatePasswordUserRequest_UsernameEqualsEmail_ThrowsArgumentException()
    {
        // Setup
        var request = new CreatePasswordUserRequest
        {
            UserName = "test@example.com",
            Email = "test@example.com",
            Password = "securepassword123",
            CreatedBy = "system"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _validator.ValidatePasswordUserRequest(request));
        
        Assert.Contains("Username and email address cannot be identical", exception.Message);
    }

    [Fact(DisplayName = "Should throw when password is empty")]
    public void ValidatePasswordUserRequest_EmptyPassword_ThrowsArgumentException()
    {
        // Setup
        var request = new CreatePasswordUserRequest
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "",
            CreatedBy = "system"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _validator.ValidatePasswordUserRequest(request));
        
        Assert.Contains("Password cannot be empty", exception.Message);
    }

    [Fact(DisplayName = "Should throw when request is null")]
    public void ValidatePasswordUserRequest_NullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _validator.ValidatePasswordUserRequest(null!));
    }

    #endregion

    #region ValidateExternalUserRequest Tests

    [Fact(DisplayName = "Should validate successfully with valid external user request")]
    public void ValidateExternalUserRequest_ValidRequest_NoExceptionThrown()
    {
        // Setup
        var request = new CreateExternalUserRequest
        {
            UserName = "externaluser",
            Email = "external@example.com",
            Provider = "Google",
            ProviderId = "google123",
            CreatedBy = "system"
        };

        // Act & Assert - Should not throw
        _validator.ValidateExternalUserRequest(request);
    }

    [Fact(DisplayName = "Should throw when provider is unsupported")]
    public void ValidateExternalUserRequest_UnsupportedProvider_ThrowsArgumentException()
    {
        // Setup
        var request = new CreateExternalUserRequest
        {
            UserName = "externaluser",
            Email = "external@example.com",
            Provider = "UnsupportedProvider",
            ProviderId = "provider123",
            CreatedBy = "system"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _validator.ValidateExternalUserRequest(request));
        
        Assert.Contains("is not supported", exception.Message);
        Assert.Contains("UnsupportedProvider", exception.Message);
    }

    [Fact(DisplayName = "Should throw when username and email are identical")]
    public void ValidateExternalUserRequest_UsernameEqualsEmail_ThrowsArgumentException()
    {
        // Setup
        var request = new CreateExternalUserRequest
        {
            UserName = "test@example.com",
            Email = "test@example.com",
            Provider = "Google",
            ProviderId = "google123",
            CreatedBy = "system"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _validator.ValidateExternalUserRequest(request));
        
        Assert.Contains("Username and email address cannot be identical", exception.Message);
    }

    #endregion

    #region ValidateAuthenticationMethod Tests

    [Fact(DisplayName = "Should validate successfully when password is provided")]
    public void ValidateAuthenticationMethod_PasswordProvided_NoExceptionThrown()
    {
        // Setup
        var request = new AdminCreateUserRequest
        {
            UserName = "adminuser",
            Email = "admin@example.com",
            Password = "adminpassword123",
            CreatedBy = "administrator"
        };

        // Act & Assert - Should not throw
        _validator.ValidateAuthenticationMethod(request);
    }

    [Fact(DisplayName = "Should validate successfully when external provider is provided")]
    public void ValidateAuthenticationMethod_ExternalProviderProvided_NoExceptionThrown()
    {
        // Setup
        var request = new AdminCreateUserRequest
        {
            UserName = "adminuser",
            Email = "admin@example.com",
            Provider = "Google",
            ProviderId = "google123",
            CreatedBy = "administrator"
        };

        // Act & Assert - Should not throw
        _validator.ValidateAuthenticationMethod(request);
    }

    [Fact(DisplayName = "Should throw when neither password nor external provider provided")]
    public void ValidateAuthenticationMethod_NoAuthenticationMethod_ThrowsArgumentException()
    {
        // Setup
        var request = new AdminCreateUserRequest
        {
            UserName = "adminuser",
            Email = "admin@example.com",
            CreatedBy = "administrator"
            // No password, no provider
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _validator.ValidateAuthenticationMethod(request));
        
        Assert.Contains("Either password or external provider", exception.Message);
    }

    [Fact(DisplayName = "Should throw when provider specified but provider ID missing")]
    public void ValidateAuthenticationMethod_ProviderWithoutProviderId_ThrowsArgumentException()
    {
        // Setup
        var request = new AdminCreateUserRequest
        {
            UserName = "adminuser",
            Email = "admin@example.com",
            Provider = "Google",
            // ProviderId is missing
            CreatedBy = "administrator"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _validator.ValidateAuthenticationMethod(request));
        
        Assert.Contains("Both Provider and ProviderId must be specified", exception.Message);
    }

    #endregion

    #region ValidateAdminUserRequest Tests

    [Fact(DisplayName = "Should validate successfully with valid admin user request")]
    public void ValidateAdminUserRequest_ValidRequest_NoExceptionThrown()
    {
        // Setup
        var request = new AdminCreateUserRequest
        {
            UserName = "adminuser",
            Email = "admin@example.com",
            Password = "adminpassword123",
            CreatedBy = "administrator"
        };

        // Act & Assert - Should not throw
        _validator.ValidateAdminUserRequest(request);
    }

    [Fact(DisplayName = "Should throw when external provider is unsupported")]
    public void ValidateAdminUserRequest_UnsupportedExternalProvider_ThrowsArgumentException()
    {
        // Setup
        var request = new AdminCreateUserRequest
        {
            UserName = "adminuser",
            Email = "admin@example.com",
            Provider = "UnsupportedProvider",
            ProviderId = "provider123",
            CreatedBy = "administrator"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => _validator.ValidateAdminUserRequest(request));
        
        Assert.Contains("is not supported", exception.Message);
        Assert.Contains("UnsupportedProvider", exception.Message);
    }

    #endregion
}
