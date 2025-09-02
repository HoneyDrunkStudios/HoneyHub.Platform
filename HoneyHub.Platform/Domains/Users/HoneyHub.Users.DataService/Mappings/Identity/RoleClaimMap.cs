using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Identity;

public class RoleClaimMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RoleClaimEntity>();
        entity.ToTable("RoleClaim");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ClaimType).HasMaxLength(256);
        entity.HasOne(x => x.Role)
              .WithMany(r => r.Claims)
              .HasForeignKey(x => x.RoleId)
              .HasConstraintName("FK_RoleClaim_Role")
              .OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(x => x.RoleId).HasDatabaseName("IX_RoleClaim_RoleId");
    }
}
