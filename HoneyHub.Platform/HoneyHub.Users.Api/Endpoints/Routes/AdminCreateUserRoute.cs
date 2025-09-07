using HoneyHub.Users.Api.Endpoints.Shared;
using HoneyHub.Users.Api.Sdk.Requests;
using HoneyHub.Users.AppService.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace HoneyHub.Users.Api.Endpoints.Routes;

/// <summary>
/// Administrative user creation endpoint following RESTful conventions.
/// Matches SDK AdminCreateAsync method expectations with simple Guid response.
/// TODO: Add authorization middleware to restrict access to administrators only.
/// </summary>
public static class AdminCreateUserRoute
{
    /// <summary>
    /// Maps the administrative user creation endpoint at POST /api/user/admin.
    /// </summary>
    public static RouteHandlerBuilder MapAdminCreateUser(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/admin", HandleAsync)
                       .WithName("AdminCreateUser")
                       .WithSummary("Create a new user with administrative privileges")
                       .WithDescription("Creates a new user account with administrative configuration options. Returns the user's public ID on success.")
                       .Produces<Guid>(StatusCodes.Status201Created)
                       .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
                       .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                       .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                       .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Handles administrative user creation requests.
    /// Returns only PublicId on success to match SDK expectations.
    /// </summary>
    private static async Task<IResult> HandleAsync(
        [FromBody] AdminCreateUserRequest request,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Add authorization check here using shared helper
            // Example implementation:
            // if (!httpContext.User.IsInRole("Administrator"))
            // {
            //     return EndpointResponseHelpers.CreateForbiddenResponse(
            //         "Administrator privileges required for this operation.");
            // }

            // Validate request model using shared validation logic
            if (!EndpointResponseHelpers.ValidateRequest(request, out var validationErrors))
            {
                return EndpointResponseHelpers.CreateValidationProblemResponse(
                    validationErrors,
                    "One or more validation errors occurred during administrative user creation.");
            }

            // Create user via application service
            var userId = await userService.AdminCreateUserAsync(request, cancellationToken);

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
                "An unexpected error occurred during administrative user creation.");
        }
    }
}
