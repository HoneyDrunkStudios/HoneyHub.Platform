using HoneyHub.Core.DataService.Entities;
using HoneyHub.Users.Api.Sdk.Models;
using HoneyHub.Users.Api.Sdk.Models.Users;
using HoneyHub.Users.DataService.Entities.Subscriptions;
using HoneyHub.Users.DataService.Entities.Users;
using Mapster;

namespace HoneyHub.Users.AppService.Mapping;

/// <summary>
/// Registers Mapster mappings for the Users domain.
/// This includes entity-to-model and model-to-entity conversions,
/// while ensuring sensitive fields (e.g. password hashes) are never mapped.
/// </summary>
public sealed class UsersMappings : IRegister
{
    /// <summary>
    /// Configure type mappings for Mapster.
    /// </summary>
    public void Register(TypeAdapterConfig config)
    {
        // ─── Base types ────────────────────────────────────────────────
        // Audit fields (Id, CreatedBy, etc.) share names, so map by convention.
        config.NewConfig<BaseEntity, BaseModel>();
        config.NewConfig<BaseModel, BaseEntity>();

        // ─── Subscription Plan ─────────────────────────────────────────
        // Required because it's a nested object on User.
        config.NewConfig<SubscriptionPlanEntity, SubscriptionPlan>();
        config.NewConfig<SubscriptionPlan, SubscriptionPlanEntity>();

        // ─── UserEntity → User (Entity -> DTO) ─────────────────────────
        // Inherits base mappings. Convention covers most properties.
        config.NewConfig<UserEntity, User>()
              .Inherits<BaseEntity, BaseModel>();

        // ─── User → UserEntity (DTO -> Entity) ─────────────────────────
        // Never map secrets (password) or identity-managed fields.
        config.NewConfig<User, UserEntity>()
              .Inherits<BaseModel, BaseEntity>()
              .Ignore(nameof(UserEntity.PasswordHash))
              .Ignore(nameof(UserEntity.SecurityStamp))
              .Ignore(nameof(UserEntity.ConcurrencyStamp))
              .Ignore(nameof(UserEntity.EmailConfirmed))
              .Ignore(nameof(UserEntity.PhoneNumberConfirmed))
              .Ignore(nameof(UserEntity.TwoFactorEnabled))
              .Ignore(nameof(UserEntity.LockoutEnd))
              .Ignore(nameof(UserEntity.LockoutEnabled))
              .Ignore(nameof(UserEntity.AccessFailedCount))
              // Navigation collections should never be set via DTO mapping
              .Ignore(nameof(UserEntity.Roles))
              .Ignore(nameof(UserEntity.Claims))
              .Ignore(nameof(UserEntity.Logins))
              .Ignore(nameof(UserEntity.Tokens))
              .Ignore(nameof(UserEntity.RefreshTokens));
    }
}
