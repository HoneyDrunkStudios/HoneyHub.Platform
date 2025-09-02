using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Identity;

public class UserClaimMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserClaimEntity>();
        entity.ToTable("UserClaim");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ClaimType).HasMaxLength(256);
        entity.HasIndex(x => x.UserId).HasDatabaseName("IX_UserClaim_UserId");
    }
}
