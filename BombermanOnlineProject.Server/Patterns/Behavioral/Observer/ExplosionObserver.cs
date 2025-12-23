namespace BombermanOnlineProject.Server.Patterns.Behavioral.Observer
{
	public class ExplosionObserver : IGameObserver
	{
		private readonly Dictionary<string, int> _playerExplosionCount;
		private readonly Dictionary<string, int> _totalCellsAffected;
		private readonly List<ExplosionEvent> _explosionHistory;
		private readonly object _explosionLock;
		private readonly string _sessionId;
		private const int MAX_HISTORY_SIZE = 100;

		public ExplosionObserver(string sessionId)
		{
			_playerExplosionCount = new Dictionary<string, int>();
			_totalCellsAffected = new Dictionary<string, int>();
			_explosionHistory = new List<ExplosionEvent>();
			_explosionLock = new object();
			_sessionId = sessionId;
		}

		public void OnPlayerKilled(string killerId, string victimId, int newScore)
		{
		}

		public void OnEnemyKilled(string playerId, string enemyId, int scoreGained)
		{
		}

		public void OnExplosionCreated(string ownerId, int centerX, int centerY, List<(int X, int Y)> affectedCells)
		{
			lock (_explosionLock)
			{
				if (!_playerExplosionCount.ContainsKey(ownerId))
					_playerExplosionCount[ownerId] = 0;
				if (!_totalCellsAffected.ContainsKey(ownerId))
					_totalCellsAffected[ownerId] = 0;

				_playerExplosionCount[ownerId]++;
				_totalCellsAffected[ownerId] += affectedCells.Count;

				var explosionEvent = new ExplosionEvent
				{
					OwnerId = ownerId,
					CenterX = centerX,
					CenterY = centerY,
					AffectedCellsCount = affectedCells.Count,
					Timestamp = DateTime.UtcNow
				};

				_explosionHistory.Add(explosionEvent);

				if (_explosionHistory.Count > MAX_HISTORY_SIZE)
				{
					_explosionHistory.RemoveAt(0);
				}

				Console.WriteLine($"[ExplosionObserver] Explosion by {ownerId.Substring(0, 8)} at ({centerX}, {centerY}). " +
								  $"Affected: {affectedCells.Count} cells. Total explosions: {_playerExplosionCount[ownerId]}");
			}
		}

		public void OnPowerUpCollected(string playerId, string powerUpType, int newValue)
		{
		}

		public void OnBombPlaced(string playerId, int x, int y, int power)
		{
			Console.WriteLine($"[ExplosionObserver] Bomb placed by {playerId.Substring(0, 8)} at ({x}, {y}) with power {power}");
		}

		public void OnWallDestroyed(int x, int y, string wallType, bool hadPowerUp)
		{
			lock (_explosionLock)
			{
				var powerUpInfo = hadPowerUp ? " (contained power-up)" : "";
				Console.WriteLine($"[ExplosionObserver] {wallType} wall destroyed at ({x}, {y}){powerUpInfo}");
			}
		}

		public void OnGameStateChanged(string sessionId, string newState)
		{
			Console.WriteLine($"[ExplosionObserver] Game state changed to: {newState}");

			if (newState == "Finished")
			{
				DisplayExplosionStatistics();
			}
		}

		public string GetObserverName()
		{
			return "ExplosionObserver";
		}

		public int GetPlayerExplosionCount(string playerId)
		{
			lock (_explosionLock)
			{
				return _playerExplosionCount.TryGetValue(playerId, out int count) ? count : 0;
			}
		}

		public int GetTotalCellsAffected(string playerId)
		{
			lock (_explosionLock)
			{
				return _totalCellsAffected.TryGetValue(playerId, out int total) ? total : 0;
			}
		}

		public double GetAverageExplosionSize(string playerId)
		{
			lock (_explosionLock)
			{
				int explosionCount = GetPlayerExplosionCount(playerId);
				if (explosionCount == 0) return 0;

				int totalCells = GetTotalCellsAffected(playerId);
				return (double)totalCells / explosionCount;
			}
		}

		public List<ExplosionEvent> GetRecentExplosions(int count = 10)
		{
			lock (_explosionLock)
			{
				return _explosionHistory
					.OrderByDescending(e => e.Timestamp)
					.Take(count)
					.ToList();
			}
		}

		public string GetExplosionStatistics()
		{
			lock (_explosionLock)
			{
				var stats = new System.Text.StringBuilder();
				stats.AppendLine("=== EXPLOSION STATISTICS ===");

				foreach (var (playerId, explosionCount) in _playerExplosionCount.OrderByDescending(kvp => kvp.Value))
				{
					int totalCells = GetTotalCellsAffected(playerId);
					double avgSize = GetAverageExplosionSize(playerId);

					stats.AppendLine($"Player {playerId.Substring(0, 8)}: " +
								   $"{explosionCount} explosions | " +
								   $"{totalCells} cells affected | " +
								   $"Avg: {avgSize:F2} cells/explosion");
				}

				return stats.ToString();
			}
		}

		private void DisplayExplosionStatistics()
		{
			Console.WriteLine("\n" + GetExplosionStatistics());
		}

		public void ResetStatistics()
		{
			lock (_explosionLock)
			{
				_playerExplosionCount.Clear();
				_totalCellsAffected.Clear();
				_explosionHistory.Clear();
				Console.WriteLine("[ExplosionObserver] All explosion statistics reset");
			}
		}
	}
}