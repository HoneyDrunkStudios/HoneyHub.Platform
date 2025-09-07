using FluentValidation;

namespace HoneyHub.Users.Api.Endpoints.Shared;

/// <summary>
/// Helper class for FluentValidation integration with API endpoints.
/// Provides standardized validation logic using FluentValidation instead of data annotations.
/// Maintains consistent error response format across all endpoints.
/// </summary>
public static class FluentValidationHelpers
{
    /// <summary>
    /// Validates request model using FluentValidation and returns formatted validation errors.
    /// Integrates with existing endpoint response patterns for consistent error handling.
    /// </summary>
    /// <typeparam name="T">Type of request model to validate</typeparam>
    /// <param name="request">Request instance to validate</param>
    /// <param name="validator">FluentValidation validator for the request type</param>
    /// <param name="validationErrors">Output parameter containing validation errors formatted for API response</param>
    /// <returns>True if validation passes, false otherwise</returns>
    public static bool ValidateRequest<T>(T request, IValidator<T> validator, out IDictionary<string, string[]> validationErrors)
    {
        var validationResult = validator.Validate(request);

        if (validationResult.IsValid)
        {
            validationErrors = new Dictionary<string, string[]>();
            return true;
        }

        // Group validation errors by property name for consistent API response format
        validationErrors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray()
            );

        return false;
    }

    /// <summary>
    /// Validates request model and creates appropriate IResult response.
    /// Encapsulates validation logic and response creation for cleaner endpoint code.
    /// </summary>
    /// <typeparam name="T">Type of request model to validate</typeparam>
    /// <param name="request">Request instance to validate</param>
    /// <param name="validator">FluentValidation validator for the request type</param>
    /// <param name="validationContextDescription">Description of the validation context for error messages</param>
    /// <returns>ValidationProblem result if validation fails, null if validation passes</returns>
    public static IResult? ValidateAndCreateResponse<T>(T request, IValidator<T> validator, string validationContextDescription)
    {
        if (ValidateRequest(request, validator, out var validationErrors))
        {
            return null; // Validation passed
        }

        return EndpointResponseHelpers.CreateValidationProblemResponse(
            validationErrors,
            $"One or more validation errors occurred during {validationContextDescription}.");
    }
}
