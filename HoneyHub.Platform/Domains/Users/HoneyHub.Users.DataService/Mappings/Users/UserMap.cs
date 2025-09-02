using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Identity;
using HoneyHub.Users.DataService.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Users;

public class UserMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserEntity>();
        entity.ToTable("User");
        entity.HasKey(x => x.Id);

        // Columns and constraints aligned with dbo.User
        entity.Property(x => x.PublicId).IsRequired();
        entity.Property(x => x.UserName).HasMaxLength(256).IsRequired();
        entity.Property(x => x.NormalizedUserName).HasMaxLength(256).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
        entity.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
        entity.Property(x => x.EmailConfirmed).HasDefaultValue(false);
        entity.Property(x => x.PasswordHash).HasMaxLength(512);
        entity.Property(x => x.SecurityStamp).HasMaxLength(40).IsRequired();
        entity.Property(x => x.ConcurrencyStamp).HasMaxLength(40).IsRequired();
        entity.Property(x => x.PhoneNumber).HasMaxLength(32);
        entity.Property(x => x.PhoneNumberConfirmed).HasDefaultValue(false);
        entity.Property(x => x.TwoFactorEnabled).HasDefaultValue(false);
        entity.Property(x => x.LockoutEnabled).HasDefaultValue(true);
        entity.Property(x => x.AccessFailedCount).HasDefaultValue(0);
        entity.Property(x => x.LastLoginAt);
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        entity.Property(x => x.SubscriptionPlanId).HasDefaultValue(1);

        // Unique indexes
        entity.HasIndex(x => x.PublicId).IsUnique().HasDatabaseName("UK_User_PublicId");
        entity.HasIndex(x => x.NormalizedUserName).IsUnique().HasDatabaseName("UK_User_NormalizedUserName");
        entity.HasIndex(x => x.NormalizedEmail).IsUnique().HasDatabaseName("UK_User_NormalizedEmail");

        // Relationships
        entity.HasOne(x => x.SubscriptionPlan)
              .WithMany(p => p.Users)
              .HasForeignKey(x => x.SubscriptionPlanId)
              .HasConstraintName("FK_User_SubscriptionPlan")
              .OnDelete(DeleteBehavior.NoAction);

        // Identity navigation relationships
        modelBuilder.Entity<UserRoleEntity>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UserId)
            .HasConstraintName("FK_UserRole_User")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserClaimEntity>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.Claims)
            .HasForeignKey(uc => uc.UserId)
            .HasConstraintName("FK_UserClaim_User")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserLoginEntity>()
            .HasOne(ul => ul.User)
            .WithMany(u => u.Logins)
            .HasForeignKey(ul => ul.UserId)
            .HasConstraintName("FK_UserLogin_User")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserTokenEntity>()
            .HasOne(ut => ut.User)
            .WithMany(u => u.Tokens)
            .HasForeignKey(ut => ut.UserId)
            .HasConstraintName("FK_UserToken_User")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshTokenEntity>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .HasConstraintName("FK_RefreshToken_User")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
