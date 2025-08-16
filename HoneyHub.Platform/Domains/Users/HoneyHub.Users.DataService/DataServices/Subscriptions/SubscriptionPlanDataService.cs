using HoneyHub.Core.DataService.Context;
using HoneyHub.Core.DataServices.DataServices;
using HoneyHub.Users.DataService.Entities.Subscriptions;

namespace HoneyHub.Users.DataService.DataServices.Subscriptions;

public class SubscriptionPlanDataService : Repository<SubscriptionPlanEntity>, ISubscriptionPlanDataService
{
    public SubscriptionPlanDataService(BaseContext context) : base(context)
    {
    }
}
