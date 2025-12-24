using BombermanOnlineProject.Server.Data.Models;
using BombermanOnlineProject.Server.Services;

namespace BombermanOnlineProject.Server.Presenters
{
	public class LeaderboardPresenter
	{
		private readonly LeaderboardService _leaderboardService;
		private readonly StatisticsService _statisticsService;

		public LeaderboardPresenter(LeaderboardService leaderboardService, StatisticsService statisticsService)
		{
			_leaderboardService = leaderboardService ?? throw new ArgumentNullException(nameof(leaderboardService));
			_statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
		}

		public async Task<IEnumerable<User>> GetTopPlayersByWinsAsync(int count = 10)
		{
			try
			{
				return await _leaderboardService.GetTopPlayersByWinsAsync(count);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting top players by wins: {ex.Message}");
				return Enumerable.Empty<User>();
			}
		}

		public async Task<IEnumerable<User>> GetTopPlayersByScoreAsync(int count = 10)
		{
			try
			{
				return await _leaderboardService.GetTopPlayersByScoreAsync(count);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting top players by score: {ex.Message}");
				return Enumerable.Empty<User>();
			}
		}

		public async Task<IEnumerable<User>> GetTopPlayersByKillsAsync(int count = 10)
		{
			try
			{
				return await _leaderboardService.GetTopPlayersByKillsAsync(count);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting top players by kills: {ex.Message}");
				return Enumerable.Empty<User>();
			}
		}

		public async Task<IEnumerable<HighScore>> GetGlobalHighScoresAsync(int count = 10)
		{
			try
			{
				return await _leaderboardService.GetGlobalHighScoresAsync(count);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting global high scores: {ex.Message}");
				return Enumerable.Empty<HighScore>();
			}
		}

		public async Task<IEnumerable<HighScore>> GetUserHighScoresAsync(int userId, int count = 10)
		{
			try
			{
				return await _leaderboardService.GetUserHighScoresAsync(userId, count);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting user high scores: {ex.Message}");
				return Enumerable.Empty<HighScore>();
			}
		}

		public async Task<IEnumerable<HighScore>> GetHighScoresByThemeAsync(string theme, int count = 10)
		{
			try
			{
				return await _leaderboardService.GetHighScoresByThemeAsync(theme, count);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting high scores by theme: {ex.Message}");
				return Enumerable.Empty<HighScore>();
			}
		}

		public async Task<IEnumerable<HighScore>> GetRecentHighScoresAsync(int count = 10)
		{
			try
			{
				return await _leaderboardService.GetRecentHighScoresAsync(count);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting recent high scores: {ex.Message}");
				return Enumerable.Empty<HighScore>();
			}
		}

		public async Task<Dictionary<string, object>> GetLeaderboardSummaryAsync()
		{
			try
			{
				return await _leaderboardService.GetLeaderboardSummaryAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting leaderboard summary: {ex.Message}");
				return new Dictionary<string, object>();
			}
		}

		public async Task<int> GetUserRankByScoreAsync(int userId)
		{
			try
			{
				return await _leaderboardService.GetUserRankByScoreAsync(userId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting user rank by score: {ex.Message}");
				return -1;
			}
		}

		public async Task<int> GetUserRankByWinsAsync(int userId)
		{
			try
			{
				return await _leaderboardService.GetUserRankByWinsAsync(userId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting user rank by wins: {ex.Message}");
				return -1;
			}
		}

		public async Task<int> GetUserRankByKillsAsync(int userId)
		{
			try
			{
				return await _leaderboardService.GetUserRankByKillsAsync(userId);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting user rank by kills: {ex.Message}");
				return -1;
			}
		}

		public async Task<Dictionary<string, object>> GetUserCompleteStatsAsync(int userId)
		{
			try
			{
				var performanceStats = await _statisticsService.GetUserPerformanceStatsAsync(userId);
				var scoreRank = await GetUserRankByScoreAsync(userId);
				var winsRank = await GetUserRankByWinsAsync(userId);
				var killsRank = await GetUserRankByKillsAsync(userId);
				var recentGames = await _statisticsService.GetRecentGamesAsync(userId, 5);
				var bestGame = await _statisticsService.GetBestGameAsync(userId);
				var themeStats = await _statisticsService.GetThemeStatisticsAsync(userId);

				var completeStats = new Dictionary<string, object>(performanceStats)
				{
					{ "ScoreRank", scoreRank },
					{ "WinsRank", winsRank },
					{ "KillsRank", killsRank },
					{ "RecentGames", recentGames },
					{ "BestGame", bestGame ?? new object() },
					{ "ThemeStatistics", themeStats }
				};

				return completeStats;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting user complete stats: {ex.Message}");
				return new Dictionary<string, object>();
			}
		}

		public async Task<bool> IsNewHighScoreAsync(int userId, int score)
		{
			try
			{
				return await _leaderboardService.IsNewHighScoreAsync(userId, score);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error checking new high score: {ex.Message}");
				return false;
			}
		}

		public async Task<bool> IsTopNScoreAsync(int score, int n = 10)
		{
			try
			{
				return await _leaderboardService.IsTopNScoreAsync(score, n);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error checking top N score: {ex.Message}");
				return false;
			}
		}

		public async Task<Dictionary<string, object>> GetLeaderboardByCategory(string category, int count = 10)
		{
			try
			{
				var result = new Dictionary<string, object>();

				switch (category.ToLower())
				{
					case "score":
						result["Players"] = await GetTopPlayersByScoreAsync(count);
						result["Category"] = "Highest Score";
						break;

					case "wins":
						result["Players"] = await GetTopPlayersByWinsAsync(count);
						result["Category"] = "Most Wins";
						break;

					case "kills":
						result["Players"] = await GetTopPlayersByKillsAsync(count);
						result["Category"] = "Most Kills";
						break;

					default:
						result["Players"] = await GetTopPlayersByScoreAsync(count);
						result["Category"] = "Highest Score";
						break;
				}

				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LeaderboardPresenter] Error getting leaderboard by category: {ex.Message}");
				return new Dictionary<string, object>
				{
					{ "Players", Enumerable.Empty<User>() },
					{ "Category", category }
				};
			}
		}
	}
}