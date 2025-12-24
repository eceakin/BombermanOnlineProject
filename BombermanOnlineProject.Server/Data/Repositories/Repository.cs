using BombermanOnlineProject.Server.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

namespace BombermanOnlineProject.Server.Data.Repositories
{
	public class Repository<T> : IRepository<T> where T : class
	{
		protected readonly BombermanDbContext _context;
		protected readonly DbSet<T> _dbSet;

		public Repository(BombermanDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_dbSet = _context.Set<T>();
		}

		public virtual async Task<T?> GetByIdAsync(int id)
		{
			return await _dbSet.FindAsync(id);
		}

		public virtual async Task<IEnumerable<T>> GetAllAsync()
		{
			return await _dbSet.ToListAsync();
		}

		public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
		{
			return await _dbSet.Where(predicate).ToListAsync();
		}

		public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
		{
			return await _dbSet.FirstOrDefaultAsync(predicate);
		}

		public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
		{
			return await _dbSet.AnyAsync(predicate);
		}

		public virtual async Task<int> CountAsync()
		{
			return await _dbSet.CountAsync();
		}

		public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
		{
			return await _dbSet.CountAsync(predicate);
		}

		public virtual async Task<T> AddAsync(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			await _dbSet.AddAsync(entity);
			return entity;
		}

		public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			var entityList = entities.ToList();
			await _dbSet.AddRangeAsync(entityList);
			return entityList;
		}

		public virtual Task UpdateAsync(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			_dbSet.Attach(entity);
			_context.Entry(entity).State = EntityState.Modified;
			return Task.CompletedTask;
		}

		public virtual Task UpdateRangeAsync(IEnumerable<T> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			foreach (var entity in entities)
			{
				_dbSet.Attach(entity);
				_context.Entry(entity).State = EntityState.Modified;
			}

			return Task.CompletedTask;
		}

		public virtual Task DeleteAsync(T entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			if (_context.Entry(entity).State == EntityState.Detached)
			{
				_dbSet.Attach(entity);
			}

			_dbSet.Remove(entity);
			return Task.CompletedTask;
		}

		public virtual Task DeleteRangeAsync(IEnumerable<T> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));

			_dbSet.RemoveRange(entities);
			return Task.CompletedTask;
		}

		public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
		{
			if (pageNumber < 1)
				throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));

			if (pageSize < 1)
				throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

			return await _dbSet
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}

		public virtual async Task<IEnumerable<T>> GetPagedAsync(
			int pageNumber,
			int pageSize,
			Expression<Func<T, bool>>? filter = null,
			Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
			string includeProperties = "")
		{
			if (pageNumber < 1)
				throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));

			if (pageSize < 1)
				throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

			IQueryable<T> query = _dbSet;

			if (filter != null)
			{
				query = query.Where(filter);
			}

			foreach (var includeProperty in includeProperties.Split
				(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				query = query.Include(includeProperty.Trim());
			}

			if (orderBy != null)
			{
				query = orderBy(query);
			}

			return await query
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}

		public virtual IQueryable<T> Query()
		{
			return _dbSet.AsQueryable();
		}

		public virtual async Task<bool> ExistsAsync(int id)
		{
			var entity = await _dbSet.FindAsync(id);
			return entity != null;
		}
	}
}
