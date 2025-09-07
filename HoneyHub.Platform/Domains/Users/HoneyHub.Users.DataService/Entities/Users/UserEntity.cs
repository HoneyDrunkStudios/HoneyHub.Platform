using HoneyHub.Core.DataService.Entities;
using HoneyHub.Users.DataService.Entities.Identity;
using HoneyHub.Users.DataService.Entities.Subscriptions;

namespace HoneyHub.Users.DataService.Entities.Users;

public class UserEntity : BaseEntity
{
    // Public-facing identifier (non-enumerable)
    public required Guid PublicId { get; set; }

    // Identity core
    public required string UserName { get; set; }
    public required string NormalizedUserName { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; } = false;

    // Authentication & security
    public string? PasswordHash { get; set; }
    public required string SecurityStamp { get; set; }
    public required string ConcurrencyStamp { get; set; }

    // Contact
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; } = false;

    // MFA & lockout
    public bool TwoFactorEnabled { get; set; } = false;
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; } = true;
    public int AccessFailedCount { get; set; } = 0;

    // Domain flags
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    // Subscription Management
    public int SubscriptionPlanId { get; set; }

    // Audit Fields (additional to BaseEntity)
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties
    public SubscriptionPlanEntity? SubscriptionPlan { get; set; }

    // Identity navigation collections
    public ICollection<UserRoleEntity> Roles { get; set; } = [];
    public ICollection<UserClaimEntity> Claims { get; set; } = [];
    public ICollection<UserLoginEntity> Logins { get; set; } = [];
    public ICollection<UserTokenEntity> Tokens { get; set; } = [];
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];
}
