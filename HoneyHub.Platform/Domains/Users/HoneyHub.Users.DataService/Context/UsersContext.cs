using HoneyHub.Core.DataService.Context;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Context;

public class UsersContext(DbContextOptions<UsersContext> dbContextOptions) : BaseContext(dbContextOptions)
{
}
