using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace HoneyHub.Users.AppService.Mapping;

/// <summary>
/// Registers Mapster for the Users domain:
///  - Builds a TypeAdapterConfig and scans this assembly for IRegister (e.g., UsersMappings)
///  - Applies global defaults
///  - Exposes IMapper (ServiceMapper) through DI
/// </summary>
public static class MappingServiceExtensions
{
    public static IServiceCollection AddUsersMappings(this IServiceCollection services)
    {
        // Build and configure a dedicated TypeAdapterConfig
        var config = new TypeAdapterConfig();

        // Scan this assembly for IRegister implementations (UsersMappings)
        config.Scan([typeof(UsersMappings).Assembly]);

        // Global conventions (adjust as you like)
        config.Default
              .IgnoreNullValues(true)
              .PreserveReference(true)
              .NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);

        // Register config + mapper
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
