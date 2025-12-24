using BombermanOnlineProject.Server.Data.Models;
using BombermanOnlineProject.Server.Data.UnitOfWork;

namespace BombermanOnlineProject.Server.Services
{
	public class StatisticsService
	{
		private readonly IUnitOfWork _unitOfWork;

		public StatisticsService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public async Task<GameStatistic> RecordGameStatisticAsync(
			int userId,
			string sessionId,
			bool isWinner,
			int finalScore,
			int kills,
			int deaths,
			int bombsPlaced,
			int wallsDestroyed,
			int powerUpsCollected,
			int roundsWon,
			int roundsLost,
			TimeSpan gameDuration,
			string? opponentUsername = null,
			string? mapTheme = null)
		{
			var statistic = new GameStatistic
			{
				UserId = userId,
				SessionId = sessionId,
				PlayedAt = DateTime.UtcNow,
				IsWinner = isWinner,
				FinalScore = finalScore,
				Kills = kills,
				Deaths = deaths,
				BombsPlaced = bombsPlaced,
				WallsDestroyed = wallsDestroyed,
				PowerUpsCollected = powerUpsCollected,
				RoundsWon = roundsWon,
				RoundsLost = roundsLost,
				GameDuration = gameDuration,
				OpponentUsername = opponentUsername,
				MapTheme = mapTheme
			};

			await _unitOfWork.GameStatistics.AddAsync(statistic);

			await _unitOfWork.Users.UpdateUserStatisticsAsync(
				userId,
				gamesPlayed: 1,
				wins: isWinner ? 1 : 0,
				losses: isWinner ? 0 : 1,
				kills: kills,
				deaths: deaths,
				score: finalScore
			);

			await _unitOfWork.SaveChangesAsync();

			Console.WriteLine($"[StatisticsService] Game statistic recorded for user {userId}");
			return statistic;
		}

		public async Task<IEnumerable<GameStatistic>> GetUserGameHistoryAsync(int userId, int pageNumber = 1, int pageSize = 20)
		{
			return await _unitOfWork.GameStatistics.GetByUserIdPagedAsync(userId, pageNumber, pageSize);
		}

		public async Task<IEnumerable<GameStatistic>> GetRecentGamesAsync(int userId, int count = 10)
		{
			return await _unitOfWork.GameStatistics.GetRecentGamesAsync(userId, count);
		}

		public async Task<Dictionary<string, object>> GetUserPerformanceStatsAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetUserPerformanceStatsAsync(userId);
		}

		public async Task<GameStatistic?> GetBestGameAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetBestGameAsync(userId);
		}

		public async Task<GameStatistic?> GetWorstGameAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetWorstGameAsync(userId);
		}

		public async Task<IEnumerable<GameStatistic>> GetWinningGamesAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetWinningGamesAsync(userId);
		}

		public async Task<IEnumerable<GameStatistic>> GetLosingGamesAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetLosingGamesAsync(userId);
		}

		public async Task<double> GetWinRateAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetWinRateAsync(userId);
		}

		public async Task<double> GetKillDeathRatioAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetKillDeathRatioAsync(userId);
		}

		public async Task<double> GetAverageScoreAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetAverageScoreAsync(userId);
		}

		public async Task<int> GetTotalGamesPlayedAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetTotalGamesPlayedAsync(userId);
		}

		public async Task<int> GetTotalKillsAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetTotalKillsAsync(userId);
		}

		public async Task<int> GetTotalDeathsAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetTotalDeathsAsync(userId);
		}

		public async Task<Dictionary<string, int>> GetThemeStatisticsAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetThemeStatisticsAsync(userId);
		}

		public async Task<Dictionary<string, int>> GetOpponentStatisticsAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetOpponentStatisticsAsync(userId);
		}

		public async Task<IEnumerable<GameStatistic>> GetGamesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
		{
			return await _unitOfWork.GameStatistics.GetGamesByDateRangeAsync(userId, startDate, endDate);
		}

		public async Task<IEnumerable<GameStatistic>> GetGamesByThemeAsync(int userId, string theme)
		{
			return await _unitOfWork.GameStatistics.GetGamesByThemeAsync(userId, theme);
		}

		public async Task<IEnumerable<GameStatistic>> GetGamesByOpponentAsync(int userId, string opponentUsername)
		{
			return await _unitOfWork.GameStatistics.GetGamesByOpponentAsync(userId, opponentUsername);
		}

		public async Task<GameStatistic?> GetLongestGameAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetLongestGameAsync(userId);
		}

		public async Task<GameStatistic?> GetShortestGameAsync(int userId)
		{
			return await _unitOfWork.GameStatistics.GetShortestGameAsync(userId);
		}

		public async Task<Dictionary<int, int>> GetDailyActivityAsync(int userId, int days = 30)
		{
			return await _unitOfWork.GameStatistics.GetDailyActivityAsync(userId, days);
		}

		public async Task<bool> HasPlayedAgainstAsync(int userId, string opponentUsername)
		{
			return await _unitOfWork.GameStatistics.HasPlayedAgainstAsync(userId, opponentUsername);
		}

		public async Task<int> GetWinsAgainstOpponentAsync(int userId, string opponentUsername)
		{
			return await _unitOfWork.GameStatistics.GetWinsAgainstOpponentAsync(userId, opponentUsername);
		}

		public async Task<int> GetLossesAgainstOpponentAsync(int userId, string opponentUsername)
		{
			return await _unitOfWork.GameStatistics.GetLossesAgainstOpponentAsync(userId, opponentUsername);
		}

		public async Task DeleteOldStatisticsAsync(DateTime beforeDate)
		{
			await _unitOfWork.GameStatistics.DeleteOldStatisticsAsync(beforeDate);
			await _unitOfWork.SaveChangesAsync();
			Console.WriteLine($"[StatisticsService] Deleted statistics before {beforeDate}");
		}
	}
}