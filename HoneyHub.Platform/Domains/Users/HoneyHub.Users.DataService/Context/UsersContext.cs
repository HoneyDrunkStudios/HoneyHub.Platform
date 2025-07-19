using HoneyHub.Core.DataService.Context;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Context;

public class UsersContext : BaseContext
{
	public UsersContext(DbContextOptions<UsersContext> dbContextOptions) : base(dbContextOptions) {	}
}
