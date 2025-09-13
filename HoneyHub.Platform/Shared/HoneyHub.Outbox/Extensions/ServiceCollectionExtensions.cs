using HoneyHub.Outbox.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HoneyHub.Outbox.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxStore<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<Abstractions.IOutboxStore, OutboxStore<TDbContext>>();
        return services;
    }
}
