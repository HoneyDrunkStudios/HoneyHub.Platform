using HoneyHub.Core.DataService.DataServices;
using HoneyHub.Users.DataService.Entities.Users;

namespace HoneyHub.Users.DataService.DataServices.Users;

public interface IUserDataService : IRepository<UserEntity>
{
    /// <summary>
    /// Checks whether a user with the given normalized username already exists.
    /// Expects the input to be pre-normalized (Trim + ToUpperInvariant) by the caller.
    /// </summary>
    /// <param name="normalizedUserName">The normalized username to check for existence.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True when a user with the same normalized username exists.</returns>
    Task<bool> UserNameExistsAsync(string normalizedUserName, CancellationToken cancellationToken = default);
}
