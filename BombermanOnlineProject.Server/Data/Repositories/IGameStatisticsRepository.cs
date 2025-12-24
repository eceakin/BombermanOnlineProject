using BombermanOnlineProject.Server.Data.Models;

namespace BombermanOnlineProject.Server.Data.Repositories
{
	public interface IGameStatisticsRepository : IRepository<GameStatistic>
	{
		Task<IEnumerable<GameStatistic>> GetByUserIdAsync(int userId);

		Task<IEnumerable<GameStatistic>> GetByUserIdPagedAsync(int userId, int pageNumber, int pageSize);

		Task<GameStatistic?> GetBySessionIdAsync(string sessionId);

		Task<IEnumerable<GameStatistic>> GetRecentGamesAsync(int userId, int count);

		Task<IEnumerable<GameStatistic>> GetWinningGamesAsync(int userId);

		Task<IEnumerable<GameStatistic>> GetLosingGamesAsync(int userId);

		Task<IEnumerable<GameStatistic>> GetGamesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);

		Task<IEnumerable<GameStatistic>> GetGamesByThemeAsync(int userId, string theme);

		Task<IEnumerable<GameStatistic>> GetGamesByOpponentAsync(int userId, string opponentUsername);

		Task<GameStatistic?> GetBestGameAsync(int userId);

		Task<GameStatistic?> GetWorstGameAsync(int userId);

		Task<GameStatistic?> GetLongestGameAsync(int userId);

		Task<GameStatistic?> GetShortestGameAsync(int userId);

		Task<GameStatistic?> GetMostKillsGameAsync(int userId);

		Task<GameStatistic?> GetMostBombsGameAsync(int userId);

		Task<GameStatistic?> GetMostWallsDestroyedGameAsync(int userId);

		Task<int> GetTotalGamesPlayedAsync(int userId);

		Task<int> GetTotalWinsAsync(int userId);

		Task<int> GetTotalLossesAsync(int userId);

		Task<int> GetTotalKillsAsync(int userId);

		Task<int> GetTotalDeathsAsync(int userId);

		Task<int> GetTotalBombsPlacedAsync(int userId);

		Task<int> GetTotalWallsDestroyedAsync(int userId);

		Task<int> GetTotalPowerUpsCollectedAsync(int userId);

		Task<double> GetAverageScoreAsync(int userId);

		Task<double> GetAverageKillsPerGameAsync(int userId);

		Task<double> GetAverageDeathsPerGameAsync(int userId);

		Task<double> GetAverageGameDurationAsync(int userId);

		Task<double> GetWinRateAsync(int userId);

		Task<double> GetKillDeathRatioAsync(int userId);

		Task<Dictionary<string, int>> GetThemeStatisticsAsync(int userId);

		Task<Dictionary<string, int>> GetOpponentStatisticsAsync(int userId);

		Task<Dictionary<string, object>> GetUserPerformanceStatsAsync(int userId);

		Task<IEnumerable<GameStatistic>> GetTopScoringGamesAsync(int userId, int count);

		Task<IEnumerable<GameStatistic>> GetRecentActivityAsync(int count);

		Task<IEnumerable<GameStatistic>> GetGamesWithMinimumScoreAsync(int userId, int minScore);

		Task<bool> HasPlayedAgainstAsync(int userId, string opponentUsername);

		Task<int> GetWinsAgainstOpponentAsync(int userId, string opponentUsername);

		Task<int> GetLossesAgainstOpponentAsync(int userId, string opponentUsername);

		Task<Dictionary<int, int>> GetDailyActivityAsync(int userId, int days);

		Task DeleteOldStatisticsAsync(DateTime beforeDate);
	}
}
