using BombermanOnlineProject.Server.Data.Repositories;

namespace BombermanOnlineProject.Server.Data.UnitOfWork
{
	public interface IUnitOfWork : IDisposable
	{
		IUserRepository Users { get; }
		IGameStatisticsRepository GameStatistics { get; }
		IRepository<Models.HighScore> HighScores { get; }
		IRepository<Models.PlayerPreference> PlayerPreferences { get; }

		Task<int> SaveChangesAsync();
		Task<int> SaveChangesAsync(CancellationToken cancellationToken);
		int SaveChanges();

		Task BeginTransactionAsync();
		Task CommitTransactionAsync();
		Task RollbackTransactionAsync();
	}
}
