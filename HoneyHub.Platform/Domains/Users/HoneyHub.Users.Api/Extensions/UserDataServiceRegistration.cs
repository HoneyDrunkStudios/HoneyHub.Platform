using HoneyHub.Outbox.Extensions;
using HoneyHub.Users.DataService.Context;
using HoneyHub.Users.DataService.DataServices.Subscriptions;
using HoneyHub.Users.DataService.DataServices.Users;

namespace HoneyHub.Users.Api.Extensions;

public static class UserDataServiceRegistration
{
    public static IServiceCollection AddUsersDataServices(this IServiceCollection services)
    {
        services.AddScoped<IUserDataService, UserDataService>();
        services.AddScoped<ISubscriptionPlanDataService, SubscriptionPlanDataService>();
        services.AddOutboxStore<UsersContext>();
        return services;
    }
}
