using HoneyHub.Core.DataService.Context;
using HoneyHub.Core.DataService.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Core.DataServices.DataServices;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
	protected readonly BaseContext _context;
	protected IQueryable<T> Table => _context.Set<T>();

	public Repository(BaseContext context) => _context = context ?? throw new ArgumentNullException(nameof(context));

	#region Get

	public async Task<T?> GetById(int Id) => await _context.Set<T>().SingleOrDefaultAsync(x => x.Id == Id);

	#endregion

	#region Insert

	public async Task Insert(T entity)
	{
		await _context.Set<T>().AddAsync(entity);
	}

	public async Task Insert(List<T> entities)
	{
		await _context.Set<T>().AddRangeAsync(entities);
	}

	#endregion

	#region Update

	public Task Update(T entity)
	{
		_context.Set<T>().Update(entity);
		return Task.CompletedTask;
	}

	public Task Update(List<T> entities)
	{
		_context.Set<T>().UpdateRange(entities);
		return Task.CompletedTask;
	}

	#endregion

	#region Delete

	public Task Delete(T entity)
	{
		_context.Set<T>().Remove(entity);
		return Task.CompletedTask;
	}

	public Task Delete(List<T> entities)
	{
		_context.Set<T>().RemoveRange(entities);
		return Task.CompletedTask;
	}

	#endregion

	#region Unit of Work

	/// <summary>
	/// Persists all pending changes to the database within a single transaction.
	/// This method provides explicit control over transaction boundaries, allowing
	/// multiple operations to be batched together for atomic persistence.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token for the async operation</param>
	/// <returns>The number of state entries written to the database</returns>
	public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return await _context.SaveChangesAsync(cancellationToken);
	}

	#endregion
}
