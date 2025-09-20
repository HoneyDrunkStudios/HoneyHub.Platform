using HoneyHub.Users.Api.Endpoints.Routes;

namespace HoneyHub.Users.Api.Endpoints;

/// <summary>
/// Users API endpoint mappings following RESTful conventions and SDK alignment.
/// Organizes user creation endpoints with proper HTTP semantics that match SDK expectations.
/// Uses route organization pattern following .NET standards for maintainability.
/// </summary>
public static class UsersEndpoints
{
    /// <summary>
    /// Maps Users-related endpoints under /api/user to match SDK BaseEndpoint pattern.
    /// Returns simple Guid responses for successful operations, matching SDK expectations.
    /// </summary>
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var usersGroup = app.MapGroup("/api/user")
                           .WithTags("Users")
                           .WithOpenApi();

        // Register organized route endpoints
        usersGroup.MapCreatePasswordUser();   // POST /api/user (matches SDK CreateAsync)
        usersGroup.MapCreateExternalUser();   // POST /api/user/external (matches SDK CreateExternalAsync)
        usersGroup.MapAdminCreateUser();      // POST /api/user/admin (matches SDK AdminCreateAsync)

        return app;
    }
}
