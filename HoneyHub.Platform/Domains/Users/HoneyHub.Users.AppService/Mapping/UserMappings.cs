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
        // Never map secrets (password/refresh token).
        config.NewConfig<User, UserEntity>()
              .Inherits<BaseModel, BaseEntity>()
              .Ignore(nameof(UserEntity.PasswordHash))
              .Ignore(nameof(UserEntity.RefreshTokenHash));
    }
}
