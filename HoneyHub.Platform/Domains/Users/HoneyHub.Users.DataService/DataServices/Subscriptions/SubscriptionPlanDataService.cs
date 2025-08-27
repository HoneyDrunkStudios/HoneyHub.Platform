using HoneyHub.Core.DataService.Context;
using HoneyHub.Core.DataService.DataServices;
using HoneyHub.Users.DataService.Entities.Subscriptions;

namespace HoneyHub.Users.DataService.DataServices.Subscriptions;

public class SubscriptionPlanDataService(BaseContext context) : Repository<SubscriptionPlanEntity>(context), ISubscriptionPlanDataService
{
}
