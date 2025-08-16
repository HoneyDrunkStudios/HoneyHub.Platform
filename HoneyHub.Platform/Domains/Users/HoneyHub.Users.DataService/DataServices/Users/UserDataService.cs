using HoneyHub.Core.DataService.Context;
using HoneyHub.Core.DataServices.DataServices;
using HoneyHub.Users.DataService.Entities.Users;

namespace HoneyHub.Users.DataService.DataServices.Users;

public class UserDataService : Repository<UserEntity>, IUserDataService
{
    public UserDataService(BaseContext context) : base(context)
    {
    }
}
