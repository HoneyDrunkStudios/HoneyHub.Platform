using HoneyHub.Core.DataService.Context;
using HoneyHub.Core.DataService.Entities;
using HoneyHub.Core.DataServices.DataServices;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.DataServices.DataServices;

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
		await _context.SaveChangesAsync();
	}

	public async Task Insert(List<T> entities)
	{
		await _context.Set<T>().AddRangeAsync(entities);
		await _context.SaveChangesAsync();
	}

	#endregion

	#region Update

	public async Task Update(T entity)
	{
		_context.Set<T>().Update(entity);
		await _context.SaveChangesAsync();
	}

	public async Task Update(List<T> entities)
	{
		_context.Set<T>().UpdateRange(entities);
		await _context.SaveChangesAsync();
	}

	#endregion

	#region Delete

	public async Task Delete(T entity)
	{
		_context.Set<T>().Remove(entity);
		await _context.SaveChangesAsync();
	}

	public async Task Delete(List<T> entities)
	{
		_context.Set<T>().RemoveRange(entities);
		await _context.SaveChangesAsync();
	}

	#endregion
}
