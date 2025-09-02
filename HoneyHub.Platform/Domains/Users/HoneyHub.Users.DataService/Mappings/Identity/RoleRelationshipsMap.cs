using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Identity;

public class RoleRelationshipsMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        // Configure Role -> UserRole and Role -> RoleClaim relationships
        modelBuilder.Entity<UserRoleEntity>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(ur => ur.RoleId)
            .HasConstraintName("FK_UserRole_Role")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoleClaimEntity>()
            .HasOne(rc => rc.Role)
            .WithMany(r => r.Claims)
            .HasForeignKey(rc => rc.RoleId)
            .HasConstraintName("FK_RoleClaim_Role")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
