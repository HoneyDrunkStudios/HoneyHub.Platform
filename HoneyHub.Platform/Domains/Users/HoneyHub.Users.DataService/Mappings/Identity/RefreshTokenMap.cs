using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Identity;

public class RefreshTokenMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RefreshTokenEntity>();
        entity.ToTable("RefreshToken");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
        entity.Property(x => x.ReplacedByTokenHash).HasMaxLength(512);
        entity.Property(x => x.CreatedByIp).HasMaxLength(45);
        entity.Property(x => x.RevokedByIp).HasMaxLength(45);

        entity.HasIndex(x => x.UserId).HasDatabaseName("IX_RefreshToken_UserId");
        entity.HasIndex(x => x.TokenHash).IsUnique().HasDatabaseName("UK_RefreshToken_TokenHash");
    }
}
