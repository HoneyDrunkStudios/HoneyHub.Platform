using HoneyHub.Core.DataService.Entities;

namespace HoneyHub.Core.DataService.DataServices;

public interface IRepository<T> where T : BaseEntity
{
    Task Insert(T entity);
    Task Update(T entity);
    Task Insert(List<T> entities);
    Task Update(List<T> entities);
    Task Delete(T entity);
    Task Delete(List<T> entities);
    Task<T?> GetById(int Id);

    /// <summary>
    /// Persists all pending changes to the database within a single transaction.
    /// This provides explicit control over transaction boundaries following the Unit of Work pattern.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
