using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Users.DataService.Entities.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.Subscriptions;

public class SubscriptionPlanMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<SubscriptionPlanEntity>();
        entity.ToTable("SubscriptionPlan");
        entity.HasKey(x => x.Id);
        entity.HasMany(x => x.Users)
            .WithOne(x => x.SubscriptionPlan)
            .HasForeignKey(x => x.SubscriptionPlanId);
    }
}
