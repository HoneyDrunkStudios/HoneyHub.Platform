using HoneyHub.Core.DataService.Entities;
using HoneyHub.Users.DataService.Entities.Users;

namespace HoneyHub.Users.DataService.Entities.Subscriptions;

public class SubscriptionPlanEntity : BaseEntity
{
    // Plan Details
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }

    // Pricing Information
    public required decimal Price { get; set; }
    public required string Currency { get; set; } = "USD";
    public required string BillingCycle { get; set; }
    public required int BillingIntervalMonths { get; set; } = 1;

    // Plan Status and Availability
    public required bool IsActive { get; set; }
    public required bool IsPublic { get; set; }
    public required bool IsDefault { get; set; }
    public required int SortOrder { get; set; }

    // Feature Limits
    public int? MaxProjects { get; set; }
    public int? MaxActiveWorkItemsPerProject { get; set; }

    // Marketing and UI
    public string? CallToAction { get; set; }
    public required bool PopularBadge { get; set; } = false;

    // Trial Configuration
    public int? TrialDays { get; set; }

    // Version Control for Price Changes
    public required int Version { get; set; } = 1;
    public required DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    // Navigation Properties
    public List<UserEntity> Users { get; set; } = [];
}
