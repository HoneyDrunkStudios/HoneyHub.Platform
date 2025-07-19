using HoneyHub.Core.DataService.Entities;
using HoneyHub.Users.DataService.Entities.Subscriptions;

namespace HoneyHub.Users.DataService.Entities.Users;

public class UserEntity : BaseEntity
{
    // Public-facing identifier (non-enumerable)
    public required Guid PublicId { get; set; }

    // Core Identity Fields
    public required string Username { get; set; }
    public required string Email { get; set; }

    // Authentication Fields
    public string? PasswordHash { get; set; }
    public string? Provider { get; set; }
    public string? ProviderId { get; set; }
    public string? RefreshTokenHash { get; set; }

    // Account Status Fields
    public required bool EmailVerified { get; set; } = false;
    public required bool IsActive { get; set; } = true;
    public required bool IsDeleted { get; set; } = false;

    // Subscription Management
    public required int SubscriptionPlanId { get; set; } = 1;

    // Audit Fields (additional to BaseEntity)
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties
    public SubscriptionPlanEntity? SubscriptionPlan { get; set; }
}
