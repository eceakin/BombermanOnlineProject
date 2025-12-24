using BombermanOnlineProject.Server.Data.Models;
using BombermanOnlineProject.Server.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BombermanOnlineProject.Server.Services
{
	public class LeaderboardService
	{
		private readonly IUnitOfWork _unitOfWork;

		public LeaderboardService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public async Task<HighScore> RecordHighScoreAsync(
			int userId,
			string sessionId,
			int score,
			int kills,
			int deaths,
			TimeSpan gameDuration,
			int roundsWon,
			string? opponentUsername = null,
			string? mapTheme = null)
		{
			var highScore = new HighScore
			{
				UserId = userId,
				SessionId = sessionId,
				Score = score,
				AchievedAt = DateTime.UtcNow,
				Kills = kills,
				Deaths = deaths,
				GameDuration = gameDuration,
				RoundsWon = roundsWon,
				OpponentUsername = opponentUsername,
				MapTheme = mapTheme
			};

			await _unitOfWork.HighScores.AddAsync(highScore);
			await _unitOfWork.Users.UpdateHighestScoreAsync(userId, score);
			await _unitOfWork.SaveChangesAsync();

			Console.WriteLine($"[LeaderboardService] High score recorded: {score} for user {userId}");
			return highScore;
		}

		public async Task<IEnumerable<User>> GetTopPlayersByWinsAsync(int count = 10)
		{
			return await _unitOfWork.Users.GetTopPlayersByWinsAsync(count);
		}

		public async Task<IEnumerable<User>> GetTopPlayersByScoreAsync(int count = 10)
		{
			return await _unitOfWork.Users.GetTopPlayersByScoreAsync(count);
		}

		public async Task<IEnumerable<User>> GetTopPlayersByKillsAsync(int count = 10)
		{
			return await _unitOfWork.Users.GetTopPlayersByKillsAsync(count);
		}

		public async Task<IEnumerable<HighScore>> GetGlobalHighScoresAsync(int count = 10)
		{
			var query = _unitOfWork.HighScores.Query()
				.OrderByDescending(h => h.Score)
				.Take(count);

			return await Task.FromResult(query.ToList());
		}

		public async Task<IEnumerable<HighScore>> GetUserHighScoresAsync(int userId, int count = 10)
		{
			var highScores = await _unitOfWork.HighScores.FindAsync(h => h.UserId == userId);
			return highScores
				.OrderByDescending(h => h.Score)
				.Take(count);
		}

		public async Task<IEnumerable<HighScore>> GetHighScoresByThemeAsync(string theme, int count = 10)
		{
			var highScores = await _unitOfWork.HighScores.FindAsync(h => h.MapTheme == theme);
			return highScores
				.OrderByDescending(h => h.Score)
				.Take(count);
		}

		public async Task<IEnumerable<HighScore>> GetRecentHighScoresAsync(int count = 10)
		{
			var query = _unitOfWork.HighScores.Query()
				.OrderByDescending(h => h.AchievedAt)
				.Take(count);

			return await Task.FromResult(query.ToList());
		}

		public async Task<int> GetUserRankByScoreAsync(int userId)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if (user == null)
			{
				return -1;
			}

			var rank = await _unitOfWork.Users.Query()
				.Where(u => u.IsActive && u.HighestScore > user.HighestScore)
				.CountAsync();

			return rank + 1;
		}

		public async Task<int> GetUserRankByWinsAsync(int userId)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if (user == null)
			{
				return -1;
			}

			var rank = await _unitOfWork.Users.Query()
				.Where(u => u.IsActive && u.TotalWins > user.TotalWins)
				.CountAsync();

			return rank + 1;
		}

		public async Task<int> GetUserRankByKillsAsync(int userId)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if (user == null)
			{
				return -1;
			}

			var rank = await _unitOfWork.Users.Query()
				.Where(u => u.IsActive && u.TotalKills > user.TotalKills)
				.CountAsync();

			return rank + 1;
		}

		public async Task<Dictionary<string, object>> GetLeaderboardSummaryAsync()
		{
			var topByScore = await GetTopPlayersByScoreAsync(5);
			var topByWins = await GetTopPlayersByWinsAsync(5);
			var topByKills = await GetTopPlayersByKillsAsync(5);
			var recentHighScores = await GetRecentHighScoresAsync(5);

			return new Dictionary<string, object>
			{
				{ "TopByScore", topByScore },
				{ "TopByWins", topByWins },
				{ "TopByKills", topByKills },
				{ "RecentHighScores", recentHighScores }
			};
		}

		public async Task<bool> IsNewHighScoreAsync(int userId, int score)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId);
			if (user == null)
			{
				return false;
			}

			return score > user.HighestScore;
		}

		public async Task<bool> IsTopNScoreAsync(int score, int n = 10)
		{
			var topScores = await GetGlobalHighScoresAsync(n);
			var topScoresList = topScores.ToList();

			if (topScoresList.Count < n)
			{
				return true;
			}

			return score > topScoresList.Last().Score;
		}
	}
}