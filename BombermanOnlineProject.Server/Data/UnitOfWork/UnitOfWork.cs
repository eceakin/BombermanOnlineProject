using BombermanOnlineProject.Server.Data.Context;
using BombermanOnlineProject.Server.Data.Models;
using BombermanOnlineProject.Server.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace BombermanOnlineProject.Server.Data.UnitOfWork
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly BombermanDbContext _context;
		private IDbContextTransaction? _transaction;

		private IUserRepository? _userRepository;
		private IGameStatisticsRepository? _gameStatisticsRepository;
		private IRepository<HighScore>? _highScoreRepository;
		private IRepository<PlayerPreference>? _playerPreferenceRepository;

		private bool _disposed = false;

		public UnitOfWork(BombermanDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public IUserRepository Users
		{
			get
			{
				_userRepository ??= new UserRepository(_context);
				return _userRepository;
			}
		}

		public IGameStatisticsRepository GameStatistics
		{
			get
			{
				_gameStatisticsRepository ??= new GameStatisticsRepository(_context);
				return _gameStatisticsRepository;
			}
		}

		public IRepository<HighScore> HighScores
		{
			get
			{
				_highScoreRepository ??= new Repository<HighScore>(_context);
				return _highScoreRepository;
			}
		}

		public IRepository<PlayerPreference> PlayerPreferences
		{
			get
			{
				_playerPreferenceRepository ??= new Repository<PlayerPreference>(_context);
				return _playerPreferenceRepository;
			}
		}

		public async Task<int> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
		{
			return await _context.SaveChangesAsync(cancellationToken);
		}

		public int SaveChanges()
		{
			return _context.SaveChanges();
		}

		public async Task BeginTransactionAsync()
		{
			_transaction = await _context.Database.BeginTransactionAsync();
		}

		public async Task CommitTransactionAsync()
		{
			try
			{
				await _context.SaveChangesAsync();

				if (_transaction != null)
				{
					await _transaction.CommitAsync();
				}
			}
			catch
			{
				await RollbackTransactionAsync();
				throw;
			}
			finally
			{
				if (_transaction != null)
				{
					await _transaction.DisposeAsync();
					_transaction = null;
				}
			}
		}

		public async Task RollbackTransactionAsync()
		{
			if (_transaction != null)
			{
				await _transaction.RollbackAsync();
				await _transaction.DisposeAsync();
				_transaction = null;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_transaction?.Dispose();
					_context.Dispose();
				}

				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
