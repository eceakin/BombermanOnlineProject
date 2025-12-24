using System.Linq.Expressions;

namespace BombermanOnlineProject.Server.Data.Repositories
{
	public interface IRepository<T> where T : class
	{
		Task<T?> GetByIdAsync(int id);

		Task<IEnumerable<T>> GetAllAsync();

		Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

		Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

		Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

		Task<int> CountAsync();

		Task<int> CountAsync(Expression<Func<T, bool>> predicate);

		Task<T> AddAsync(T entity);

		Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

		Task UpdateAsync(T entity);

		Task UpdateRangeAsync(IEnumerable<T> entities);

		Task DeleteAsync(T entity);

		Task DeleteRangeAsync(IEnumerable<T> entities);

		Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);

		Task<IEnumerable<T>> GetPagedAsync(
			int pageNumber,
			int pageSize,
			Expression<Func<T, bool>>? filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
			string includeProperties = "");

		IQueryable<T> Query();

		Task<bool> ExistsAsync(int id);
	}
}
