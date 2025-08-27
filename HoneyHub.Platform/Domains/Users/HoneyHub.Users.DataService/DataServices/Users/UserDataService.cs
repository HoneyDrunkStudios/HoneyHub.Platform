using HoneyHub.Core.DataService.Context;
using HoneyHub.Core.DataService.DataServices;
using HoneyHub.Users.DataService.Entities.Users;

namespace HoneyHub.Users.DataService.DataServices.Users;

public class UserDataService(BaseContext context) : Repository<UserEntity>(context), IUserDataService
{
}
