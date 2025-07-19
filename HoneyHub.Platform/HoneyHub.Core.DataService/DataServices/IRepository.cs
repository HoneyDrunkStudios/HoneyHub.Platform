using HoneyHub.Core.DataService.Entities;

namespace HoneyHub.Core.DataServices.DataServices;

public interface IRepository<T> where T : BaseEntity
{
	Task Insert(T entity);
	Task Update(T entity);
	Task Insert(List<T> entities);
	Task Update(List<T> entities);
	Task Delete(T entity);
	Task Delete(List<T> entities);
	Task<T?> GetById(int Id);
}