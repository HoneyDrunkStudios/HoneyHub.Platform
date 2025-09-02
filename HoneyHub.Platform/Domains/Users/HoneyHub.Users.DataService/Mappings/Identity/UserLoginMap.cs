using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Identity;

public class UserLoginMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserLoginEntity>();
        entity.ToTable("UserLogin");
        entity.HasKey(x => new { x.LoginProvider, x.ProviderKey });
        entity.Property(x => x.LoginProvider).HasMaxLength(128).IsRequired();
        entity.Property(x => x.ProviderKey).HasMaxLength(256).IsRequired();
        entity.Property(x => x.ProviderDisplayName).HasMaxLength(256);
        entity.HasIndex(x => x.UserId).HasDatabaseName("IX_UserLogin_UserId");
    }
}
