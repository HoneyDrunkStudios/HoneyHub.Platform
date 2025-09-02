using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Identity;

public class RoleMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RoleEntity>();
        entity.ToTable("Role");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
        entity.Property(x => x.NormalizedName).HasMaxLength(256).IsRequired();
        entity.Property(x => x.ConcurrencyStamp).HasMaxLength(40).IsRequired().IsConcurrencyToken();
        entity.HasIndex(x => x.NormalizedName).IsUnique().HasDatabaseName("UK_Role_NormalizedName");
    }
}
