using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Identity;

public class UserTokenMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserTokenEntity>();
        entity.ToTable("UserToken");
        entity.HasKey(x => new { x.UserId, x.LoginProvider, x.Name });
        entity.Property(x => x.LoginProvider).HasMaxLength(128).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
    }
}
