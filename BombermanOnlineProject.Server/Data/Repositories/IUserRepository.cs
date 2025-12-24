using BombermanOnlineProject.Server.Data.Models;

namespace BombermanOnlineProject.Server.Data.Repositories
{
	public interface IUserRepository : IRepository<User>
	{
		Task<User?> GetByUsernameAsync(string username);

		Task<User?> GetByEmailAsync(string email);

		Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);

		Task<User?> GetUserWithPreferencesAsync(int userId);

		Task<User?> GetUserWithStatisticsAsync(int userId);

		Task<User?> GetUserWithHighScoresAsync(int userId);

		Task<User?> GetUserCompleteAsync(int userId);

		Task<bool> UsernameExistsAsync(string username);

		Task<bool> EmailExistsAsync(string email);

		Task<IEnumerable<User>> GetTopPlayersByWinsAsync(int count);

		Task<IEnumerable<User>> GetTopPlayersByScoreAsync(int count);

		Task<IEnumerable<User>> GetTopPlayersByKillsAsync(int count);

		Task<IEnumerable<User>> GetRecentlyActiveUsersAsync(int count);

		Task<IEnumerable<User>> GetActiveUsersAsync();

		Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);

		Task UpdateLastLoginAsync(int userId);

		Task UpdateUserStatisticsAsync(int userId, int gamesPlayed, int wins, int losses, int kills, int deaths, int score);

		Task IncrementGamesPlayedAsync(int userId);

		Task IncrementWinsAsync(int userId);

		Task IncrementLossesAsync(int userId);

		Task UpdateHighestScoreAsync(int userId, int score);

		Task<Dictionary<string, object>> GetUserStatsAsync(int userId);

		Task<double> GetWinRateAsync(int userId);

		Task<double> GetKillDeathRatioAsync(int userId);

		Task DeactivateUserAsync(int userId);

		Task ActivateUserAsync(int userId);
	}
}
