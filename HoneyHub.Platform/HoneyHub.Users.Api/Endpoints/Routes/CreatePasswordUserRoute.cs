using FluentValidation;
using HoneyHub.Users.Api.Endpoints.Shared;
using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.AppService.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace HoneyHub.Users.Api.Endpoints.Routes;

/// <summary>
/// Password user creation endpoint following RESTful conventions.
/// Matches SDK CreateAsync method expectations with simple Guid response.
/// Integrates FluentValidation for comprehensive input validation.
/// </summary>
public static class CreatePasswordUserRoute
{
    /// <summary>
    /// Maps the password user creation endpoint at POST /api/user.
    /// </summary>
    public static RouteHandlerBuilder MapCreatePasswordUser(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/", HandleAsync)
                       .WithName("CreatePasswordUser")
                       .WithSummary("Create a new user with password authentication")
                       .WithDescription("Creates a new user account using password-based authentication. Returns the user's public ID on success.")
                       .Produces<Guid>(StatusCodes.Status201Created)
                       .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
                       .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Handles password user creation requests.
    /// Returns only PublicId on success to match SDK expectations.
    /// Uses FluentValidation for comprehensive input validation.
    /// </summary>
    private static async Task<IResult> HandleAsync(
        [FromBody] CreatePasswordUserRequest request,
        IValidator<CreatePasswordUserRequest> validator,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate request model using FluentValidation
            var validationResult = FluentValidationHelpers.ValidateAndCreateResponse(
                request,
                validator,
                "password user creation");

            if (validationResult is not null)
            {
                return validationResult;
            }

            // Create user via application service
            var userId = await userService.CreatePasswordUserAsync(request, cancellationToken);

            // Return successful creation response using shared helper
            return EndpointResponseHelpers.CreateSuccessfulCreationResponse(userId, "/api/user/{0}");
        }
        catch (ArgumentException ex)
        {
            return EndpointResponseHelpers.CreateBusinessRuleViolationResponse(ex.Message);
        }
        catch (Exception)
        {
            return EndpointResponseHelpers.CreateInternalServerErrorResponse(
                "An unexpected error occurred during user creation.");
        }
    }
}
