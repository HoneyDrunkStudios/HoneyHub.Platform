using HoneyHub.Users.AppService.Mapping;
using HoneyHub.Users.AppService.Services.SecurityServices;
using HoneyHub.Users.AppService.Services.Users;
using HoneyHub.Users.AppService.Services.Validators.Users;

namespace HoneyHub.Users.Api.Extensions;

public static class UserAppServiceRegistration
{
    public static IServiceCollection AddUsersAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserServiceValidator, UserServiceValidator>();
        services.AddUsersMappings();
        services.AddPasswordServices(configuration);
        return services;
    }
}
