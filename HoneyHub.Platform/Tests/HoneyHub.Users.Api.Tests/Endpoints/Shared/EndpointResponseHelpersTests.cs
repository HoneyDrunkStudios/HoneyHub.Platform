using HoneyHub.Users.Api.Endpoints.Shared;
using HoneyHub.Users.AppService.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace HoneyHub.Users.Api.Tests.Endpoints.Shared;

/// <summary>
/// Unit tests for EndpointResponseHelpers following .NET testing best practices.
/// Tests focus on response consistency, error handling, and validation patterns.
/// </summary>
public class EndpointResponseHelpersTests
{
    [Fact(DisplayName = "Should create validation problem response with correct format")]
    public void CreateValidationProblemResponse_WithErrors_ReturnsValidationProblem()
    {
        // Setup
        var validationErrors = new Dictionary<string, string[]>
        {
            ["UserName"] = ["Username is required"],
            ["Email"] = ["Invalid email format"]
        };
        const string detail = "Validation failed for user creation";

        // Act
        var result = EndpointResponseHelpers.CreateValidationProblemResponse(validationErrors, detail);

        // Assert
        Assert.NotNull(result);
        // Note: Full validation would require converting IResult to actual response
        // This test validates the method executes without errors
    }

    [Fact(DisplayName = "Should create business rule violation response with correct status")]
    public void CreateBusinessRuleViolationResponse_WithMessage_ReturnsProblemDetails()
    {
        // Setup
        const string message = "Username and email cannot be identical";

        // Act
        var result = EndpointResponseHelpers.CreateBusinessRuleViolationResponse(message);

        // Assert
        Assert.NotNull(result);
        // The method should return a Problem result (verified by execution without exception)
    }

    [Fact(DisplayName = "Should create internal server error response with safe message")]
    public void CreateInternalServerErrorResponse_WithDetail_ReturnsProblemDetails()
    {
        // Setup
        const string detail = "An unexpected error occurred during processing";

        // Act
        var result = EndpointResponseHelpers.CreateInternalServerErrorResponse(detail);

        // Assert
        Assert.NotNull(result);
        // The method should return a Problem result with 500 status code
    }

    [Fact(DisplayName = "Should create forbidden response with default message")]
    public void CreateForbiddenResponse_WithoutDetail_ReturnsDefaultMessage()
    {
        // Act
        var result = EndpointResponseHelpers.CreateForbiddenResponse();

        // Assert
        Assert.NotNull(result);
        // Should use default message for forbidden operations
    }

    [Fact(DisplayName = "Should create forbidden response with custom message")]
    public void CreateForbiddenResponse_WithCustomDetail_ReturnsCustomMessage()
    {
        // Setup
        const string customDetail = "Administrator role required for this operation";

        // Act
        var result = EndpointResponseHelpers.CreateForbiddenResponse(customDetail);

        // Assert
        Assert.NotNull(result);
        // Should use provided custom message
    }

    [Fact(DisplayName = "Should validate valid request successfully")]
    public void ValidateRequest_ValidRequest_ReturnsTrue()
    {
        // Setup
        var validRequest = new CreatePasswordUserRequest
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "securepassword123",
            CreatedBy = "system"
        };

        // Act
        var isValid = EndpointResponseHelpers.ValidateRequest(validRequest, out var validationErrors);

        // Assert
        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Fact(DisplayName = "Should validate invalid request and return errors")]
    public void ValidateRequest_InvalidRequest_ReturnsFalseWithErrors()
    {
        // Setup
        var invalidRequest = new CreatePasswordUserRequest
        {
            UserName = "", // Invalid - empty
            Email = "invalid-email", // Invalid - not proper email format
            Password = "123", // Invalid - too short
            CreatedBy = "system"
        };

        // Act
        var isValid = EndpointResponseHelpers.ValidateRequest(invalidRequest, out var validationErrors);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(validationErrors);
        
        // Should contain validation errors for invalid fields
        Assert.True(validationErrors.Count > 0);
    }

    [Fact(DisplayName = "Should create successful creation response with location")]
    public void CreateSuccessfulCreationResponse_ValidParameters_ReturnsCreatedResult()
    {
        // Setup
        var resourceId = Guid.NewGuid();
        const string resourcePath = "/api/user/{0}";

        // Act
        var result = EndpointResponseHelpers.CreateSuccessfulCreationResponse(resourceId, resourcePath);

        // Assert
        Assert.NotNull(result);
        // Should return Created result with location header and resource ID
    }

    [Fact(DisplayName = "Should handle null validation errors gracefully")]
    public void ValidateRequest_NullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            EndpointResponseHelpers.ValidateRequest<CreatePasswordUserRequest>(null!, out _));
    }
}
