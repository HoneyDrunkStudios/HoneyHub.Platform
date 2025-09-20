using HoneyHub.Core.DataService.Context;
using HoneyHub.Core.DataService.DataServices;
using HoneyHub.Users.DataService.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.DataServices.Users;

public class UserDataService(BaseContext context) : Repository<UserEntity>(context), IUserDataService
{
    // Read-only query for users. Default AsNoTracking for performance on read paths.
    private IQueryable<UserEntity> Users => Table.AsNoTracking();

    /// <summary>
    /// Checks whether a user with the given normalized username already exists.
    /// Assumes the caller has already normalized the username.
    /// </summary>
    /// <param name="normalizedUserName">The normalized username to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<bool> UserNameExistsAsync(string normalizedUserName, CancellationToken cancellationToken = default) =>
        await Users.AnyAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken).ConfigureAwait(false);
}
