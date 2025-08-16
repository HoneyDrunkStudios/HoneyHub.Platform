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
    /// <summary>
    /// The name of the external authentication provider (e.g., Google, Facebook, etc.).
    /// Used to identify the source of the user's authentication.
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// The unique identifier assigned to the user by the external authentication provider.
    /// This is typically used to link the user account with the provider's system.
    /// </summary>
    public string? ProviderId { get; set; }
    public string? RefreshTokenHash { get; set; }

    // Account Status Fields
    public bool EmailVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    // Subscription Management
    public int SubscriptionPlanId { get; set; }

    // Audit Fields (additional to BaseEntity)
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties
    public SubscriptionPlanEntity? SubscriptionPlan { get; set; }
}
