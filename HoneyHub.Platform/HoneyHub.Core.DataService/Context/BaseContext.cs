using HoneyHub.Core.DataService.Entities;
using HoneyHub.Core.DataService.Mappings;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HoneyHub.Core.DataService.Context;

public class BaseContext : DbContext
{
	public BaseContext(DbContextOptions<BaseContext> dbContextOptions) : base(dbContextOptions)
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
		var maps = Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(x => x.GetInterfaces()
			.Contains(typeof(IEntityMap)));

		foreach (var map in maps)
		{
			var createdInstance = Activator.CreateInstance(map) as IEntityMap;

			if (createdInstance != null)
				createdInstance.Configure(modelBuilder);
		}
	}

	private void ConfigureDbSets(ModelBuilder modelBuilder)
	{
		var entities = Assembly.GetExecutingAssembly()
			.GetTypes()
			.Where(x => x.BaseType != null && x.BaseType
			.Equals(typeof(BaseEntity)));

		foreach (var entity in entities)
			modelBuilder.Entity(entity);
	}
}
