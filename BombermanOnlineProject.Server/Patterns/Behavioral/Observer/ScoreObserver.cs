namespace BombermanOnlineProject.Server.Patterns.Behavioral.Observer
{
	public class ScoreObserver : IGameObserver
	{
		private readonly Dictionary<string, int> _playerScores;
		private readonly Dictionary<string, int> _playerKills;
		private readonly Dictionary<string, int> _playerDeaths;
		private readonly object _scoreLock;
		private readonly string _sessionId;

		public ScoreObserver(string sessionId)
		{
			_playerScores = new Dictionary<string, int>();
			_playerKills = new Dictionary<string, int>();
			_playerDeaths = new Dictionary<string, int>();
			_scoreLock = new object();
			_sessionId = sessionId;
		}

		public void OnPlayerKilled(string killerId, string victimId, int newScore)
		{
			lock (_scoreLock)
			{
				if (!_playerScores.ContainsKey(killerId))
					_playerScores[killerId] = 0;
				if (!_playerKills.ContainsKey(killerId))
					_playerKills[killerId] = 0;
				if (!_playerDeaths.ContainsKey(victimId))
					_playerDeaths[victimId] = 0;

				_playerScores[killerId] = newScore;
				_playerKills[killerId]++;
				_playerDeaths[victimId]++;

				Console.WriteLine($"[ScoreObserver] Player {killerId.Substring(0, 8)} killed {victimId.Substring(0, 8)}. " +
								  $"Score: {newScore}, Total Kills: {_playerKills[killerId]}");
			}
		}

		public void OnEnemyKilled(string playerId, string enemyId, int scoreGained)
		{
			lock (_scoreLock)
			{
				if (!_playerScores.ContainsKey(playerId))
					_playerScores[playerId] = 0;

				_playerScores[playerId] += scoreGained;

				Console.WriteLine($"[ScoreObserver] Player {playerId.Substring(0, 8)} killed enemy {enemyId.Substring(0, 8)}. " +
								  $"Score gained: +{scoreGained}, Total: {_playerScores[playerId]}");
			}
		}

		public void OnExplosionCreated(string ownerId, int centerX, int centerY, List<(int X, int Y)> affectedCells)
		{
		}

		public void OnPowerUpCollected(string playerId, string powerUpType, int newValue)
		{
			lock (_scoreLock)
			{
				if (!_playerScores.ContainsKey(playerId))
					_playerScores[playerId] = 0;

				const int POWERUP_SCORE_BONUS = 10;
				_playerScores[playerId] += POWERUP_SCORE_BONUS;

				Console.WriteLine($"[ScoreObserver] Player {playerId.Substring(0, 8)} collected {powerUpType}. " +
								  $"Score: +{POWERUP_SCORE_BONUS}, Total: {_playerScores[playerId]}");
			}
		}

		public void OnBombPlaced(string playerId, int x, int y, int power)
		{
		}

		public void OnWallDestroyed(int x, int y, string wallType, bool hadPowerUp)
		{
		}

		public void OnGameStateChanged(string sessionId, string newState)
		{
			Console.WriteLine($"[ScoreObserver] Game state changed to: {newState}");

			if (newState == "Finished")
			{
				DisplayFinalScores();
			}
		}

		public string GetObserverName()
		{
			return "ScoreObserver";
		}

		public int GetPlayerScore(string playerId)
		{
			lock (_scoreLock)
			{
				return _playerScores.TryGetValue(playerId, out int score) ? score : 0;
			}
		}

		public int GetPlayerKills(string playerId)
		{
			lock (_scoreLock)
			{
				return _playerKills.TryGetValue(playerId, out int kills) ? kills : 0;
			}
		}

		public int GetPlayerDeaths(string playerId)
		{
			lock (_scoreLock)
			{
				return _playerDeaths.TryGetValue(playerId, out int deaths) ? deaths : 0;
			}
		}

		public Dictionary<string, (int Score, int Kills, int Deaths)> GetAllPlayerStats()
		{
			lock (_scoreLock)
			{
				var stats = new Dictionary<string, (int Score, int Kills, int Deaths)>();

				foreach (var playerId in _playerScores.Keys)
				{
					stats[playerId] = (
						GetPlayerScore(playerId),
						GetPlayerKills(playerId),
						GetPlayerDeaths(playerId)
					);
				}

				return stats;
			}
		}

		public string GetLeaderboard()
		{
			lock (_scoreLock)
			{
				var sortedPlayers = _playerScores
					.OrderByDescending(kvp => kvp.Value)
					.ToList();

				var leaderboard = new System.Text.StringBuilder();
				leaderboard.AppendLine("=== LEADERBOARD ===");

				int rank = 1;
				foreach (var (playerId, score) in sortedPlayers)
				{
					int kills = GetPlayerKills(playerId);
					int deaths = GetPlayerDeaths(playerId);
					leaderboard.AppendLine($"{rank}. Player {playerId.Substring(0, 8)} - " +
										   $"Score: {score} | Kills: {kills} | Deaths: {deaths}");
					rank++;
				}

				return leaderboard.ToString();
			}
		}

		private void DisplayFinalScores()
		{
			Console.WriteLine("\n" + GetLeaderboard());
		}

		public void ResetScores()
		{
			lock (_scoreLock)
			{
				_playerScores.Clear();
				_playerKills.Clear();
				_playerDeaths.Clear();
				Console.WriteLine("[ScoreObserver] All scores reset");
			}
		}
	}
}