using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Identity;

public class UserRoleMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserRoleEntity>();
        entity.ToTable("UserRole");
        entity.HasKey(x => new { x.UserId, x.RoleId });
        entity.HasIndex(x => x.RoleId).HasDatabaseName("IX_UserRole_RoleId");
    }
}
