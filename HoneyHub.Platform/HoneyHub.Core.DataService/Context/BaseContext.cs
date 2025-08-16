using HoneyHub.Core.DataService.Entities;
using HoneyHub.Core.DataService.Mappings;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HoneyHub.Core.DataService.Context;

public class BaseContext : DbContext
{
    public BaseContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
        ChangeTracker.LazyLoadingEnabled = false;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureDbSets(modelBuilder);
        ConfigureMapping(modelBuilder);
    }

    private void ConfigureMapping(ModelBuilder modelBuilder)
    {
        var maps = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(x => x.GetInterfaces()
            .Contains(typeof(IEntityMap)));

        foreach (var map in maps)
        {
            var createdInstance = Activator.CreateInstance(map) as IEntityMap;

            if (createdInstance != null)
                createdInstance.Configure(modelBuilder);
        }
    }

    private static readonly Lazy<HashSet<Type>> CachedEntities = new Lazy<HashSet<Type>>(() =>
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.BaseType != null && type.BaseType == typeof(BaseEntity))
            .ToHashSet();
    });

    private void ConfigureDbSets(ModelBuilder modelBuilder)
    {
        foreach (var entity in CachedEntities.Value)
        {
            modelBuilder.Entity(entity);
        }
    }
}
