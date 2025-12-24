using BombermanOnlineProject.Server.Data.Context;
using Microsoft.EntityFrameworkCore;
using BombermanOnlineProject.Server.Data.Models;

namespace BombermanOnlineProject.Server.Data.Repositories
{
	public class GameStatisticsRepository : Repository<GameStatistic>, IGameStatisticsRepository
	{
		public GameStatisticsRepository(BombermanDbContext context) : base(context)
		{
		}

		public async Task<IEnumerable<GameStatistic>> GetByUserIdAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderByDescending(g => g.PlayedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<GameStatistic>> GetByUserIdPagedAsync(int userId, int pageNumber, int pageSize)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderByDescending(g => g.PlayedAt)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}

		public async Task<GameStatistic?> GetBySessionIdAsync(string sessionId)
		{
			if (string.IsNullOrWhiteSpace(sessionId))
				throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

			return await _dbSet
				.Include(g => g.User)
				.FirstOrDefaultAsync(g => g.SessionId == sessionId);
		}

		public async Task<IEnumerable<GameStatistic>> GetRecentGamesAsync(int userId, int count)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderByDescending(g => g.PlayedAt)
				.Take(count)
				.ToListAsync();
		}

		public async Task<IEnumerable<GameStatistic>> GetWinningGamesAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId && g.IsWinner)
				.OrderByDescending(g => g.PlayedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<GameStatistic>> GetLosingGamesAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId && !g.IsWinner)
				.OrderByDescending(g => g.PlayedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<GameStatistic>> GetGamesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
		{
			return await _dbSet
				.Where(g => g.UserId == userId && g.PlayedAt >= startDate && g.PlayedAt <= endDate)
				.OrderByDescending(g => g.PlayedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<GameStatistic>> GetGamesByThemeAsync(int userId, string theme)
		{
			if (string.IsNullOrWhiteSpace(theme))
				return Enumerable.Empty<GameStatistic>();

			return await _dbSet
				.Where(g => g.UserId == userId && g.MapTheme == theme)
				.OrderByDescending(g => g.PlayedAt)
				.ToListAsync();
		}

		public async Task<IEnumerable<GameStatistic>> GetGamesByOpponentAsync(int userId, string opponentUsername)
		{
			if (string.IsNullOrWhiteSpace(opponentUsername))
				return Enumerable.Empty<GameStatistic>();

			return await _dbSet
				.Where(g => g.UserId == userId && g.OpponentUsername == opponentUsername)
				.OrderByDescending(g => g.PlayedAt)
				.ToListAsync();
		}

		public async Task<GameStatistic?> GetBestGameAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderByDescending(g => g.FinalScore)
				.ThenByDescending(g => g.Kills)
				.ThenBy(g => g.Deaths)
				.FirstOrDefaultAsync();
		}

		public async Task<GameStatistic?> GetWorstGameAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderBy(g => g.FinalScore)
				.ThenBy(g => g.Kills)
				.ThenByDescending(g => g.Deaths)
				.FirstOrDefaultAsync();
		}

		public async Task<GameStatistic?> GetLongestGameAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderByDescending(g => g.GameDuration)
				.FirstOrDefaultAsync();
		}

		public async Task<GameStatistic?> GetShortestGameAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderBy(g => g.GameDuration)
				.FirstOrDefaultAsync();
		}

		public async Task<GameStatistic?> GetMostKillsGameAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderByDescending(g => g.Kills)
				.FirstOrDefaultAsync();
		}

		public async Task<GameStatistic?> GetMostBombsGameAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderByDescending(g => g.BombsPlaced)
				.FirstOrDefaultAsync();
		}

		public async Task<GameStatistic?> GetMostWallsDestroyedGameAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderByDescending(g => g.WallsDestroyed)
				.FirstOrDefaultAsync();
		}

		public async Task<int> GetTotalGamesPlayedAsync(int userId)
		{
			return await _dbSet
				.CountAsync(g => g.UserId == userId);
		}

		public async Task<int> GetTotalWinsAsync(int userId)
		{
			return await _dbSet
				.CountAsync(g => g.UserId == userId && g.IsWinner);
		}

		public async Task<int> GetTotalLossesAsync(int userId)
		{
			return await _dbSet
				.CountAsync(g => g.UserId == userId && !g.IsWinner);
		}

		public async Task<int> GetTotalKillsAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.SumAsync(g => g.Kills);
		}

		public async Task<int> GetTotalDeathsAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.SumAsync(g => g.Deaths);
		}

		public async Task<int> GetTotalBombsPlacedAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.SumAsync(g => g.BombsPlaced);
		}

		public async Task<int> GetTotalWallsDestroyedAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.SumAsync(g => g.WallsDestroyed);
		}

		public async Task<int> GetTotalPowerUpsCollectedAsync(int userId)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.SumAsync(g => g.PowerUpsCollected);
		}

		public async Task<double> GetAverageScoreAsync(int userId)
		{
			var games = await _dbSet
				.Where(g => g.UserId == userId)
				.ToListAsync();

			if (!games.Any())
				return 0.0;

			return Math.Round(games.Average(g => g.FinalScore), 2);
		}

		public async Task<double> GetAverageKillsPerGameAsync(int userId)
		{
			var games = await _dbSet
				.Where(g => g.UserId == userId)
				.ToListAsync();

			if (!games.Any())
				return 0.0;

			return Math.Round(games.Average(g => g.Kills), 2);
		}

		public async Task<double> GetAverageDeathsPerGameAsync(int userId)
		{
			var games = await _dbSet
				.Where(g => g.UserId == userId)
				.ToListAsync();

			if (!games.Any())
				return 0.0;

			return Math.Round(games.Average(g => g.Deaths), 2);
		}

		public async Task<double> GetAverageGameDurationAsync(int userId)
		{
			var games = await _dbSet
				.Where(g => g.UserId == userId)
				.ToListAsync();

			if (!games.Any())
				return 0.0;

			return Math.Round(games.Average(g => g.GameDuration.TotalMinutes), 2);
		}

		public async Task<double> GetWinRateAsync(int userId)
		{
			var totalGames = await GetTotalGamesPlayedAsync(userId);

			if (totalGames == 0)
				return 0.0;

			var wins = await GetTotalWinsAsync(userId);
			return Math.Round((double)wins / totalGames * 100, 2);
		}

		public async Task<double> GetKillDeathRatioAsync(int userId)
		{
			var totalDeaths = await GetTotalDeathsAsync(userId);

			if (totalDeaths == 0)
			{
				var totalKills = await GetTotalKillsAsync(userId);
				return totalKills;
			}

			var kills = await GetTotalKillsAsync(userId);
			return Math.Round((double)kills / totalDeaths, 2);
		}

		public async Task<Dictionary<string, int>> GetThemeStatisticsAsync(int userId)
		{
			var games = await _dbSet
				.Where(g => g.UserId == userId && g.MapTheme != null)
				.GroupBy(g => g.MapTheme)
				.Select(group => new { Theme = group.Key, Count = group.Count() })
				.ToListAsync();

			return games.ToDictionary(x => x.Theme!, x => x.Count);
		}

		public async Task<Dictionary<string, int>> GetOpponentStatisticsAsync(int userId)
		{
			var games = await _dbSet
				.Where(g => g.UserId == userId && g.OpponentUsername != null)
				.GroupBy(g => g.OpponentUsername)
				.Select(group => new { Opponent = group.Key, Count = group.Count() })
				.ToListAsync();

			return games.ToDictionary(x => x.Opponent!, x => x.Count);
		}

		public async Task<Dictionary<string, object>> GetUserPerformanceStatsAsync(int userId)
		{
			var totalGames = await GetTotalGamesPlayedAsync(userId);
			var totalWins = await GetTotalWinsAsync(userId);
			var totalLosses = await GetTotalLossesAsync(userId);
			var totalKills = await GetTotalKillsAsync(userId);
			var totalDeaths = await GetTotalDeathsAsync(userId);
			var winRate = await GetWinRateAsync(userId);
			var kdRatio = await GetKillDeathRatioAsync(userId);
			var avgScore = await GetAverageScoreAsync(userId);
			var avgKills = await GetAverageKillsPerGameAsync(userId);
			var avgDeaths = await GetAverageDeathsPerGameAsync(userId);
			var avgDuration = await GetAverageGameDurationAsync(userId);
			var bestGame = await GetBestGameAsync(userId);

			return new Dictionary<string, object>
			{
				{ "TotalGames", totalGames },
				{ "TotalWins", totalWins },
				{ "TotalLosses", totalLosses },
				{ "TotalKills", totalKills },
				{ "TotalDeaths", totalDeaths },
				{ "WinRate", winRate },
				{ "KillDeathRatio", kdRatio },
				{ "AverageScore", avgScore },
				{ "AverageKills", avgKills },
				{ "AverageDeaths", avgDeaths },
				{ "AverageDuration", avgDuration },
				{ "BestScore", bestGame?.FinalScore ?? 0 },
				{ "BestKills", bestGame?.Kills ?? 0 }
			};
		}

		public async Task<IEnumerable<GameStatistic>> GetTopScoringGamesAsync(int userId, int count)
		{
			return await _dbSet
				.Where(g => g.UserId == userId)
				.OrderByDescending(g => g.FinalScore)
				.Take(count)
				.ToListAsync();
		}

		public async Task<IEnumerable<GameStatistic>> GetRecentActivityAsync(int count)
		{
			return await _dbSet
				.Include(g => g.User)
				.OrderByDescending(g => g.PlayedAt)
				.Take(count)
				.ToListAsync();
		}

		public async Task<IEnumerable<GameStatistic>> GetGamesWithMinimumScoreAsync(int userId, int minScore)
		{
			return await _dbSet
				.Where(g => g.UserId == userId && g.FinalScore >= minScore)
				.OrderByDescending(g => g.FinalScore)
				.ToListAsync();
		}

		public async Task<bool> HasPlayedAgainstAsync(int userId, string opponentUsername)
		{
			if (string.IsNullOrWhiteSpace(opponentUsername))
				return false;

			return await _dbSet
				.AnyAsync(g => g.UserId == userId && g.OpponentUsername == opponentUsername);
		}

		public async Task<int> GetWinsAgainstOpponentAsync(int userId, string opponentUsername)
		{
			if (string.IsNullOrWhiteSpace(opponentUsername))
				return 0;

			return await _dbSet
				.CountAsync(g => g.UserId == userId &&
					g.OpponentUsername == opponentUsername &&
					g.IsWinner);
		}

		public async Task<int> GetLossesAgainstOpponentAsync(int userId, string opponentUsername)
		{
			if (string.IsNullOrWhiteSpace(opponentUsername))
				return 0;

			return await _dbSet
				.CountAsync(g => g.UserId == userId &&
					g.OpponentUsername == opponentUsername &&
					!g.IsWinner);
		}

		public async Task<Dictionary<int, int>> GetDailyActivityAsync(int userId, int days)
		{
			var startDate = DateTime.UtcNow.AddDays(-days);

			var games = await _dbSet
				.Where(g => g.UserId == userId && g.PlayedAt >= startDate)
				.GroupBy(g => g.PlayedAt.Date)
				.Select(group => new { Date = group.Key, Count = group.Count() })
				.ToListAsync();

			return games.ToDictionary(x => (x.Date - startDate.Date).Days, x => x.Count);
		}

		public async Task DeleteOldStatisticsAsync(DateTime beforeDate)
		{
			var oldStats = await _dbSet
				.Where(g => g.PlayedAt < beforeDate)
				.ToListAsync();

			if (oldStats.Any())
			{
				_dbSet.RemoveRange(oldStats);
			}
		}
	}
}
