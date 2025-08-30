using HoneyHub.Users.AppService.Services.Users;

namespace HoneyHub.Users.Api.Endpoints;

/// <summary>
/// Users API endpoint mappings. Keeps Program.cs thin and scalable as endpoints grow.
/// </summary>
public static class UsersEndpoints
{
    /// <summary>
    /// Maps Users-related endpoints under /api/users.
    /// </summary>
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        // POST /api/users - placeholder implementation invoking current service method
        group.MapPost("/", async (IUserService users) =>
        {
            await users.CreateUser();
            return Results.Accepted();
        })
        .WithName("CreateUser");

        return app;
    }
}
