using HoneyHub.Core.DataServices.DataServices;
using HoneyHub.Users.DataService.Entities.Users;

namespace HoneyHub.Users.DataService.DataServices.Users;

public interface IUserDataService : IRepository<UserEntity>
{
}