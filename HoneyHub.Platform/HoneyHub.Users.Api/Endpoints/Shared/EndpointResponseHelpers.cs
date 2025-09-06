using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HoneyHub.Users.Api.Endpoints.Shared;

/// <summary>
/// Static helper class for creating consistent API responses across all endpoints.
/// Provides standardized error handling and response formatting patterns.
/// Follows DRY principle while maintaining clean separation of concerns.
/// </summary>
public static class EndpointResponseHelpers
{
    /// <summary>
    /// Creates standardized validation problem response for input validation errors.
    /// Provides consistent format for data annotation and business validation failures.
    /// </summary>
    /// <param name="validationErrors">Dictionary of validation errors by field name</param>
    /// <param name="detail">Detailed description of the validation failure context</param>
    /// <returns>ValidationProblem result with standardized format</returns>
    public static IResult CreateValidationProblemResponse(
        IDictionary<string, string[]> validationErrors, 
        string detail)
    {
        return Results.ValidationProblem(validationErrors, 
            title: "Validation Failed", 
            detail: detail);
    }

    /// <summary>
    /// Creates standardized business rule violation response.
    /// Used when domain validation rules are violated during processing.
    /// </summary>
    /// <param name="message">Specific business rule violation message</param>
    /// <returns>Problem details result with 400 Bad Request status</returns>
    public static IResult CreateBusinessRuleViolationResponse(string message)
    {
        return Results.Problem(
            title: "Business Rule Violation",
            detail: message,
            statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Creates standardized internal server error response.
    /// Used for unexpected exceptions that should not expose internal details.
    /// </summary>
    /// <param name="detail">Safe error message for client consumption</param>
    /// <returns>Problem details result with 500 Internal Server Error status</returns>
    public static IResult CreateInternalServerErrorResponse(string detail)
    {
        return Results.Problem(
            title: "Internal Server Error",
            detail: detail,
            statusCode: StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Creates standardized authorization failure response.
    /// Used when user lacks sufficient permissions for the requested operation.
    /// </summary>
    /// <param name="detail">Specific authorization failure message</param>
    /// <returns>Problem details result with 403 Forbidden status</returns>
    public static IResult CreateForbiddenResponse(string detail = "Insufficient permissions for this operation.")
    {
        return Results.Problem(
            title: "Forbidden",
            detail: detail,
            statusCode: StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Validates request model using data annotations and returns validation errors.
    /// Provides consistent validation logic across all endpoints.
    /// </summary>
    /// <typeparam name="T">Type of request model to validate</typeparam>
    /// <param name="request">Request instance to validate</param>
    /// <param name="validationErrors">Output parameter containing validation errors</param>
    /// <returns>True if validation passes, false otherwise</returns>
    public static bool ValidateRequest<T>(T request, out IDictionary<string, string[]> validationErrors)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request!);
        
        var isValid = Validator.TryValidateObject(request!, validationContext, validationResults, true);
        
        validationErrors = validationResults.ToDictionary(
            vr => vr.MemberNames.FirstOrDefault() ?? "Unknown",
            vr => new[] { vr.ErrorMessage ?? "Validation failed" });
        
        return isValid;
    }

    /// <summary>
    /// Creates a successful resource creation response with location header.
    /// Provides consistent format for successful POST operations.
    /// </summary>
    /// <param name="resourceId">The ID of the created resource</param>
    /// <param name="resourcePath">The path template for the created resource</param>
    /// <returns>Created result with location header and resource ID</returns>
    public static IResult CreateSuccessfulCreationResponse(Guid resourceId, string resourcePath)
    {
        var location = string.Format(resourcePath, resourceId);
        return Results.Created(location, resourceId);
    }
}
