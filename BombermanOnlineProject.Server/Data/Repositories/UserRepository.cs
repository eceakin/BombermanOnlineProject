using BombermanOnlineProject.Server.Data.Context;
using Microsoft.EntityFrameworkCore;
using BombermanOnlineProject.Server.Data.Models;

namespace BombermanOnlineProject.Server.Data.Repositories
{
	public class UserRepository : Repository<User>, IUserRepository
	{
		public UserRepository(BombermanDbContext context) : base(context)
		{
		}

		public async Task<User?> GetByUsernameAsync(string username)
		{
			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username cannot be null or empty", nameof(username));

			return await _dbSet
				.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
		}

		public async Task<User?> GetByEmailAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				throw new ArgumentException("Email cannot be null or empty", nameof(email));

			return await _dbSet
				.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
		}

		public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
		{
			if (string.IsNullOrWhiteSpace(usernameOrEmail))
				throw new ArgumentException("Username or email cannot be null or empty", nameof(usernameOrEmail));

			var lowerSearch = usernameOrEmail.ToLower();

			return await _dbSet
				.FirstOrDefaultAsync(u =>
					u.Username.ToLower() == lowerSearch ||
					u.Email.ToLower() == lowerSearch);
		}

		public async Task<User?> GetUserWithPreferencesAsync(int userId)
		{
			return await _dbSet
				.Include(u => u.PlayerPreference)
				.FirstOrDefaultAsync(u => u.UserId == userId);
		}

		public async Task<User?> GetUserWithStatisticsAsync(int userId)
		{
			return await _dbSet
				.Include(u => u.GameStatistics)
				.FirstOrDefaultAsync(u => u.UserId == userId);
		}

		public async Task<User?> GetUserWithHighScoresAsync(int userId)
		{
			return await _dbSet
				.Include(u => u.HighScores.OrderByDescending(h => h.Score).Take(10))
				.FirstOrDefaultAsync(u => u.UserId == userId);
		}

		public async Task<User?> GetUserCompleteAsync(int userId)
		{
			return await _dbSet
				.Include(u => u.PlayerPreference)
				.Include(u => u.GameStatistics.OrderByDescending(g => g.PlayedAt).Take(20))
				.Include(u => u.HighScores.OrderByDescending(h => h.Score).Take(10))
				.FirstOrDefaultAsync(u => u.UserId == userId);
		}

		public async Task<bool> UsernameExistsAsync(string username)
		{
			if (string.IsNullOrWhiteSpace(username))
				return false;

			return await _dbSet
				.AnyAsync(u => u.Username.ToLower() == username.ToLower());
		}

		public async Task<bool> EmailExistsAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
				return false;

			return await _dbSet
				.AnyAsync(u => u.Email.ToLower() == email.ToLower());
		}

		public async Task<IEnumerable<User>> GetTopPlayersByWinsAsync(int count)
		{
			return await _dbSet
				.Where(u => u.IsActive && u.TotalGamesPlayed > 0)
				.OrderByDescending(u => u.TotalWins)
				.ThenByDescending(u => u.HighestScore)
				.Take(count)
				.ToListAsync();
		}

		public async Task<IEnumerable<User>> GetTopPlayersByScoreAsync(int count)
		{
			return await _dbSet
				.Where(u => u.IsActive && u.HighestScore > 0)
				.OrderByDescending(u => u.HighestScore)
				.ThenByDescending(u => u.TotalWins)
				.Take(count)
				.ToListAsync();
		}

		public async Task<IEnumerable<User>> GetTopPlayersByKillsAsync(int count)
		{
			return await _dbSet
				.Where(u => u.IsActive && u.TotalKills > 0)
				.OrderByDescending(u => u.TotalKills)
				.ThenBy(u => u.TotalDeaths)
				.Take(count)
				.ToListAsync();
		}

		public async Task<IEnumerable<User>> GetRecentlyActiveUsersAsync(int count)
		{
			return await _dbSet
				.Where(u => u.IsActive && u.LastLoginAt.HasValue)
				.OrderByDescending(u => u.LastLoginAt)
				.Take(count)
				.ToListAsync();
		}

		public async Task<IEnumerable<User>> GetActiveUsersAsync()
		{
			return await _dbSet
				.Where(u => u.IsActive)
				.OrderBy(u => u.Username)
				.ToListAsync();
		}

		public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
		{
			if (string.IsNullOrWhiteSpace(searchTerm))
				return Enumerable.Empty<User>();

			var lowerSearch = searchTerm.ToLower();

			return await _dbSet
				.Where(u => u.IsActive &&
					(u.Username.ToLower().Contains(lowerSearch) ||
					 u.Email.ToLower().Contains(lowerSearch)))
				.OrderBy(u => u.Username)
				.Take(50)
				.ToListAsync();
		}

		public async Task UpdateLastLoginAsync(int userId)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user != null)
			{
				user.LastLoginAt = DateTime.UtcNow;
				_context.Entry(user).Property(u => u.LastLoginAt).IsModified = true;
			}
		}

		public async Task UpdateUserStatisticsAsync(
			int userId,
			int gamesPlayed,
			int wins,
			int losses,
			int kills,
			int deaths,
			int score)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user != null)
			{
				user.TotalGamesPlayed += gamesPlayed;
				user.TotalWins += wins;
				user.TotalLosses += losses;
				user.TotalKills += kills;
				user.TotalDeaths += deaths;

				if (score > user.HighestScore)
				{
					user.HighestScore = score;
				}

				_context.Entry(user).State = EntityState.Modified;
			}
		}

		public async Task IncrementGamesPlayedAsync(int userId)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user != null)
			{
				user.TotalGamesPlayed++;
				_context.Entry(user).Property(u => u.TotalGamesPlayed).IsModified = true;
			}
		}

		public async Task IncrementWinsAsync(int userId)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user != null)
			{
				user.TotalWins++;
				_context.Entry(user).Property(u => u.TotalWins).IsModified = true;
			}
		}

		public async Task IncrementLossesAsync(int userId)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user != null)
			{
				user.TotalLosses++;
				_context.Entry(user).Property(u => u.TotalLosses).IsModified = true;
			}
		}

		public async Task UpdateHighestScoreAsync(int userId, int score)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user != null && score > user.HighestScore)
			{
				user.HighestScore = score;
				_context.Entry(user).Property(u => u.HighestScore).IsModified = true;
			}
		}

		public async Task<Dictionary<string, object>> GetUserStatsAsync(int userId)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user == null)
				return new Dictionary<string, object>();

			var winRate = await GetWinRateAsync(userId);
			var kdRatio = await GetKillDeathRatioAsync(userId);

			return new Dictionary<string, object>
			{
				{ "UserId", user.UserId },
				{ "Username", user.Username },
				{ "TotalGamesPlayed", user.TotalGamesPlayed },
				{ "TotalWins", user.TotalWins },
				{ "TotalLosses", user.TotalLosses },
				{ "TotalKills", user.TotalKills },
				{ "TotalDeaths", user.TotalDeaths },
				{ "HighestScore", user.HighestScore },
				{ "WinRate", winRate },
				{ "KillDeathRatio", kdRatio },
				{ "CreatedAt", user.CreatedAt },
				{ "LastLoginAt", user.LastLoginAt ?? DateTime.MinValue }
			};
		}

		public async Task<double> GetWinRateAsync(int userId)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user == null || user.TotalGamesPlayed == 0)
				return 0.0;

			return Math.Round((double)user.TotalWins / user.TotalGamesPlayed * 100, 2);
		}

		public async Task<double> GetKillDeathRatioAsync(int userId)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user == null || user.TotalDeaths == 0)
				return user?.TotalKills ?? 0;

			return Math.Round((double)user.TotalKills / user.TotalDeaths, 2);
		}

		public async Task DeactivateUserAsync(int userId)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user != null)
			{
				user.IsActive = false;
				_context.Entry(user).Property(u => u.IsActive).IsModified = true;
			}
		}

		public async Task ActivateUserAsync(int userId)
		{
			var user = await _dbSet.FindAsync(userId);

			if (user != null)
			{
				user.IsActive = true;
				_context.Entry(user).Property(u => u.IsActive).IsModified = true;
			}
		}
	}
}
