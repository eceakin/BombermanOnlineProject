using BombermanOnline.Server.Core.Walls;
using BombermanOnlineProject.Server.Configuration;
using BombermanOnlineProject.Server.Core.Entities;
using BombermanOnlineProject.Server.Core.Entities.Enemies;
using BombermanOnlineProject.Server.Core.Map;
using BombermanOnlineProject.Server.Core.Walls;
using BombermanOnlineProject.Server.Patterns.Behavioral.Observer;
using BombermanOnlineProject.Server.Patterns.Creational.Factory;
using BombermanOnlineProject.Server.Patterns.Structural.Decorator;
using System.Collections.Concurrent;

namespace BombermanOnlineProject.Server.Core.Game
{
	public class GameSession
	{
		#region Properties

		public string SessionId { get; private set; }
		public string HostPlayerId { get; private set; }
		public GameState State { get; private set; }
		public DateTime CreatedAt { get; private set; }
		public DateTime? StartedAt { get; private set; }
		public DateTime? EndedAt { get; private set; }
		public int CurrentRound { get; private set; }
		public GameMap Map { get; private set; }

		#endregion

		#region Private Fields

		private readonly ConcurrentDictionary<string, IPlayer> _players;
		private readonly ConcurrentDictionary<string, Bomb> _activeBombs;
		private readonly ConcurrentDictionary<string, Explosion> _activeExplosions;
		private readonly ConcurrentDictionary<string, Enemy> _activeEnemies;
		private readonly ConcurrentDictionary<string, PowerUp> _activePowerUps;
		private readonly Dictionary<string, int> _roundWins;
		private readonly object _sessionLock;
		private readonly GameSubject _gameSubject;
		private readonly ScoreObserver _scoreObserver;
		private readonly ExplosionObserver _explosionObserver;
		private readonly PowerUpFactory _powerUpFactory;
		private readonly EnemyFactory _enemyFactory;
		private Timer? _gameUpdateTimer;
		private const int UPDATE_INTERVAL_MS = 33;

		#endregion

		#region Constructor

		public GameSession(string sessionId, string hostPlayerId)
		{
			SessionId = sessionId;
			HostPlayerId = hostPlayerId;
			State = GameState.Waiting;
			CreatedAt = DateTime.UtcNow;
			CurrentRound = 0;

			_players = new ConcurrentDictionary<string, IPlayer>();
			_activeBombs = new ConcurrentDictionary<string, Bomb>();
			_activeExplosions = new ConcurrentDictionary<string, Explosion>();
			_activeEnemies = new ConcurrentDictionary<string, Enemy>();
			_activePowerUps = new ConcurrentDictionary<string, PowerUp>();
			_roundWins = new Dictionary<string, int>();
			_sessionLock = new object();

			_gameSubject = new GameSubject(sessionId);
			_scoreObserver = new ScoreObserver(sessionId);
			_explosionObserver = new ExplosionObserver(sessionId);

			_gameSubject.Attach(_scoreObserver);
			_gameSubject.Attach(_explosionObserver);

			_powerUpFactory = new PowerUpFactory();
			_enemyFactory = new EnemyFactory();

			Map = new GameMap();

			Console.WriteLine($"[GameSession] Session {sessionId} created by {hostPlayerId}");
		}

		#endregion

		#region Player Management

		public bool AddPlayer(string playerId)
		{
			lock (_sessionLock)
			{
				if (State != GameState.Waiting)
				{
					Console.WriteLine($"[GameSession] Cannot add player - game already started");
					return false;
				}

				if (_players.Count >= GameSettings.MaxPlayersPerGame)
				{
					Console.WriteLine($"[GameSession] Session full");
					return false;
				}

				if (_players.ContainsKey(playerId))
				{
					Console.WriteLine($"[GameSession] Player {playerId} already in session");
					return false;
				}

				var playerNumber = _players.Count;
				var (spawnX, spawnY) = Map.GetSpawnPosition(playerNumber);
				var playerName = $"Player{playerNumber + 1}";

				var player = new Player(playerName, playerNumber, spawnX, spawnY);

				if (_players.TryAdd(playerId, player))
				{
					_roundWins[playerId] = 0;
					Console.WriteLine($"[GameSession] Player {playerId} added as {playerName}");
					return true;
				}

				return false;
			}
		}

		public void RemovePlayer(string playerId)
		{
			lock (_sessionLock)
			{
				if (_players.TryRemove(playerId, out var player))
				{
					_roundWins.Remove(playerId);
					Console.WriteLine($"[GameSession] Player {playerId} removed");

					if (_players.IsEmpty && State != GameState.Finished)
					{
						EndGame();
					}
				}
			}
		}

		public IPlayer? GetPlayer(string playerId)
		{
			_players.TryGetValue(playerId, out var player);
			return player;
		}

		public IReadOnlyList<string> GetPlayerIds()
		{
			return _players.Keys.ToList();
		}

		public bool IsEmpty()
		{
			return _players.IsEmpty;
		}

		#endregion

		#region Game State Management

		public void StartGame()
		{
			lock (_sessionLock)
			{
				if (State != GameState.Waiting)
				{
					Console.WriteLine($"[GameSession] Cannot start - invalid state: {State}");
					return;
				}

				if (_players.Count < 1)
				{
					Console.WriteLine($"[GameSession] Cannot start - not enough players");
					return;
				}

				State = GameState.Playing;
				StartedAt = DateTime.UtcNow;
				CurrentRound = 1;

				SpawnEnemies(3);

				_gameUpdateTimer = new Timer(GameUpdateLoop, null, 0, UPDATE_INTERVAL_MS);

				_gameSubject.NotifyGameStateChanged("Playing");

				Console.WriteLine($"[GameSession] Game started with {_players.Count} players");
			}
		}

		public void PauseGame()
		{
			lock (_sessionLock)
			{
				if (State == GameState.Playing)
				{
					State = GameState.Paused;
					_gameUpdateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
					_gameSubject.NotifyGameStateChanged("Paused");
				}
			}
		}

		public void ResumeGame()
		{
			lock (_sessionLock)
			{
				if (State == GameState.Paused)
				{
					State = GameState.Playing;
					_gameUpdateTimer?.Change(0, UPDATE_INTERVAL_MS);
					_gameSubject.NotifyGameStateChanged("Playing");
				}
			}
		}

		public void EndGame()
		{
			lock (_sessionLock)
			{
				State = GameState.Finished;
				EndedAt = DateTime.UtcNow;

				_gameUpdateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
				_gameUpdateTimer?.Dispose();
				_gameUpdateTimer = null;

				_gameSubject.NotifyGameStateChanged("Finished");

				Console.WriteLine($"[GameSession] Game ended. Duration: {(EndedAt.Value - StartedAt!.Value).TotalMinutes:F2} minutes");
				Console.WriteLine(_scoreObserver.GetLeaderboard());
			}
		}

		#endregion

		#region Game Loop

		private void GameUpdateLoop(object? state)
		{
			if (State != GameState.Playing) return;

			float deltaTime = UPDATE_INTERVAL_MS / 1000f;

			UpdatePlayers(deltaTime);
			UpdateBombs(deltaTime);
			UpdateExplosions(deltaTime);
			UpdateEnemies(deltaTime);
			UpdatePowerUps(deltaTime);

			CheckWinCondition();
		}

		private void UpdatePlayers(float deltaTime)
		{
			foreach (var player in _players.Values)
			{
				player.Update(deltaTime);
			}
		}

		private void UpdateBombs(float deltaTime)
		{
			var bombsToExplode = new List<string>();

			foreach (var kvp in _activeBombs)
			{
				var bomb = kvp.Value;
				bomb.Update(deltaTime);

				if (bomb.ShouldExplode())
				{
					bombsToExplode.Add(kvp.Key);
				}
			}

			foreach (var bombId in bombsToExplode)
			{
				if (_activeBombs.TryRemove(bombId, out var bomb))
				{
					CreateExplosion(bomb);
				}
			}
		}

		private void UpdateExplosions(float deltaTime)
		{
			var explosionsToRemove = new List<string>();

			foreach (var kvp in _activeExplosions)
			{
				var explosion = kvp.Value;
				explosion.Update(deltaTime);

				if (explosion.IsExpired())
				{
					explosionsToRemove.Add(kvp.Key);
				}
			}

			foreach (var explosionId in explosionsToRemove)
			{
				_activeExplosions.TryRemove(explosionId, out _);
			}
		}

		private void UpdateEnemies(float deltaTime)
		{
			foreach (var enemy in _activeEnemies.Values)
			{
				enemy.Update(deltaTime);

				var nearestPlayer = FindNearestPlayer(enemy.X, enemy.Y);
				if (nearestPlayer != null)
				{
					var nextMove = enemy.GetNextMove(Map, nearestPlayer.X, nearestPlayer.Y);
					enemy.MoveTo(nextMove.X, nextMove.Y, Map);

					if (enemy.X == nearestPlayer.X && enemy.Y == nearestPlayer.Y)
					{
						HandlePlayerEnemyCollision(nearestPlayer, enemy);
					}
				}
			}
		}

		private void UpdatePowerUps(float deltaTime)
		{
			var powerUpsToRemove = new List<string>();

			foreach (var kvp in _activePowerUps)
			{
				var powerUp = kvp.Value;
				powerUp.Update(deltaTime);

				if (powerUp.IsExpired())
				{
					powerUpsToRemove.Add(kvp.Key);
				}
			}

			foreach (var powerUpId in powerUpsToRemove)
			{
				_activePowerUps.TryRemove(powerUpId, out _);
			}
		}

		#endregion

		#region Bomb Management

		public bool PlaceBomb(string playerId)
		{
			var player = GetPlayer(playerId);
			if (player == null || !player.CanPlaceBomb())
			{
				return false;
			}

			if (!Map.GetCell(player.X, player.Y).IsWalkable())
			{
				return false;
			}

			var bomb = new Bomb(playerId, player.X, player.Y, player.BombPower);
			player.PlaceBomb();

			if (_activeBombs.TryAdd(bomb.Id, bomb))
			{
				Map.GetCell(player.X, player.Y).AddObject(bomb);
				_gameSubject.NotifyBombPlaced(playerId, player.X, player.Y, player.BombPower);
				Console.WriteLine($"[GameSession] Bomb placed by {playerId} at ({player.X}, {player.Y})");
				return true;
			}

			return false;
		}

		private void CreateExplosion(Bomb bomb)
		{
			var explosion = new Explosion(bomb.OwnerId, bomb.X, bomb.Y, bomb.Power, Map);

			if (_activeExplosions.TryAdd(explosion.Id, explosion))
			{
				_gameSubject.NotifyExplosionCreated(bomb.OwnerId, bomb.X, bomb.Y, explosion.AffectedCells);

				ProcessExplosionDamage(explosion);

				var player = GetPlayer(bomb.OwnerId);
				if (player != null)
				{
					player.BombExploded();
				}

				Map.GetCell(bomb.X, bomb.Y).RemoveObject(bomb);
			}
		}

		private void ProcessExplosionDamage(Explosion explosion)
		{
			foreach (var (x, y) in explosion.AffectedCells)
			{
				var cell = Map.GetCell(x, y);

				if (cell.HasWall() && cell.Wall!.IsBreakable)
				{
					bool wasDestroyed = cell.Wall.TakeDamage();
					if (wasDestroyed)
					{
						bool hadPowerUp = false;

						if (cell.Wall is BreakableWall bw && bw.HasPowerUp)
						{
							hadPowerUp = true;
							SpawnPowerUp(x, y);
						}
						else if (cell.Wall is HardWall hw && hw.HasPowerUp)
						{
							hadPowerUp = true;
							SpawnPowerUp(x, y);
						}

						_gameSubject.NotifyWallDestroyed(x, y, cell.Wall.GetType().Name, hadPowerUp);
						cell.RemoveWall();
					}
				}

				foreach (var player in _players.Values)
				{
					if (player.IsAlive && player.X == x && player.Y == y)
					{
						HandlePlayerExplosionDamage(player, explosion.OwnerId);
					}
				}

				var enemyToKill = _activeEnemies.Values.FirstOrDefault(e => e.IsAlive && e.X == x && e.Y == y);
				if (enemyToKill != null)
				{
					HandleEnemyExplosionDamage(enemyToKill, explosion.OwnerId);
				}
			}
		}

		#endregion

		#region Collision & Damage

		private void HandlePlayerExplosionDamage(IPlayer victim, string attackerId)
		{
			if (!victim.IsInvulnerable)
			{
				victim.TakeDamage();

				var attacker = GetPlayer(attackerId);
				if (attacker != null && attackerId != GetPlayerIdByPlayer(victim))
				{
					attacker.AddKill();
					_gameSubject.NotifyPlayerKilled(attackerId, GetPlayerIdByPlayer(victim)!, attacker.Score);
				}
			}
		}

		private void HandleEnemyExplosionDamage(Enemy enemy, string playerId)
		{
			if (_activeEnemies.TryRemove(enemy.Id, out _))
			{
				enemy.TakeDamage();

				var player = GetPlayer(playerId);
				if (player != null)
				{
					player.Score += enemy.ScoreValue;
					_gameSubject.NotifyEnemyKilled(playerId, enemy.Id, enemy.ScoreValue);
				}
			}
		}

		private void HandlePlayerEnemyCollision(IPlayer player, Enemy enemy)
		{
			if (!player.IsInvulnerable)
			{
				player.TakeDamage();
			}
		}

		#endregion

		#region Movement

		public bool MovePlayer(string playerId, int newX, int newY)
		{
			var player = GetPlayer(playerId);
			if (player == null || !player.IsAlive)
			{
				return false;
			}

			if (!Map.IsValidPosition(newX, newY) || !Map.IsWalkable(newX, newY))
			{
				return false;
			}

			int distance = Math.Abs(newX - player.X) + Math.Abs(newY - player.Y);
			if (distance != 1)
			{
				return false;
			}

			player.MoveTo(newX, newY);

			CheckPowerUpCollection(player, newX, newY);

			return true;
		}

		#endregion

		#region PowerUp Management

		private void SpawnPowerUp(int x, int y)
		{
			var powerUp = _powerUpFactory.CreateRandom(x, y);

			if (_activePowerUps.TryAdd(powerUp.Id, powerUp))
			{
				Map.GetCell(x, y).SetPowerUp(powerUp);
				Console.WriteLine($"[GameSession] PowerUp spawned at ({x}, {y}): {powerUp.PowerUpType}");
			}
		}

		private void CheckPowerUpCollection(IPlayer player, int x, int y)
		{
			var cell = Map.GetCell(x, y);
			var powerUp = cell.CollectPowerUp();

			if (powerUp != null && _activePowerUps.TryRemove(powerUp.Id, out _))
			{
				ApplyPowerUp(player, powerUp);
			}
		}

		private void ApplyPowerUp(IPlayer player, PowerUp powerUp)
		{
			var playerId = GetPlayerIdByPlayer(player);
			if (playerId == null) return;

			IPlayer enhancedPlayer = player;

			switch (powerUp.PowerUpType)
			{
				case PowerUpType.SpeedBoost:
					enhancedPlayer = new SpeedBoostDecorator(player);
					_gameSubject.NotifyPowerUpCollected(playerId, "SpeedBoost", (int)enhancedPlayer.Speed);
					break;

				case PowerUpType.BombPowerIncrease:
					enhancedPlayer = new BombPowerDecorator(player);
					_gameSubject.NotifyPowerUpCollected(playerId, "BombPower", enhancedPlayer.BombPower);
					break;

				case PowerUpType.BombCountIncrease:
					enhancedPlayer = new BombCountDecorator(player);
					_gameSubject.NotifyPowerUpCollected(playerId, "BombCount", enhancedPlayer.MaxBombs);
					break;
			}

			_players.TryUpdate(playerId, enhancedPlayer, player);
		}

		#endregion

		#region Enemy Management

		private void SpawnEnemies(int count)
		{
			var random = new Random();

			for (int i = 0; i < count; i++)
			{
				int attempts = 0;
				const int maxAttempts = 50;

				while (attempts < maxAttempts)
				{
					int x = random.Next(1, Map.Width - 1);
					int y = random.Next(1, Map.Height - 1);

					if (Map.IsWalkable(x, y) && !IsNearSpawnPoint(x, y))
					{
						var enemy = _enemyFactory.CreateRandom(x, y);
						_activeEnemies.TryAdd(enemy.Id, enemy);
						Map.GetCell(x, y).AddObject(enemy);
						Console.WriteLine($"[GameSession] Enemy spawned at ({x}, {y})");
						break;
					}

					attempts++;
				}
			}
		}

		private bool IsNearSpawnPoint(int x, int y)
		{
			foreach (var (spawnX, spawnY) in GameMap.SpawnPositions)
			{
				int distance = Math.Abs(x - spawnX) + Math.Abs(y - spawnY);
				if (distance < 4)
				{
					return true;
				}
			}
			return false;
		}

		private IPlayer? FindNearestPlayer(int enemyX, int enemyY)
		{
			IPlayer? nearest = null;
			int minDistance = int.MaxValue;

			foreach (var player in _players.Values)
			{
				if (!player.IsAlive) continue;

				int distance = Math.Abs(player.X - enemyX) + Math.Abs(player.Y - enemyY);
				if (distance < minDistance)
				{
					minDistance = distance;
					nearest = player;
				}
			}

			return nearest;
		}

		#endregion

		#region Win Condition

		private void CheckWinCondition()
		{
			var alivePlayers = _players.Values.Where(p => p.IsAlive).ToList();

			if (alivePlayers.Count == 1)
			{
				var winner = alivePlayers[0];
				var winnerId = GetPlayerIdByPlayer(winner);

				if (winnerId != null)
				{
					_roundWins[winnerId]++;
					Console.WriteLine($"[GameSession] Round {CurrentRound} won by {winnerId}");

					if (_roundWins[winnerId] >= GameSettings.RoundsToWin)
					{
						Console.WriteLine($"[GameSession] {winnerId} wins the match!");
						EndGame();
					}
					else
					{
						StartNewRound();
					}
				}
			}
			else if (alivePlayers.Count == 0)
			{
				Console.WriteLine($"[GameSession] Round {CurrentRound} ended in a draw");
				StartNewRound();
			}
		}

		private void StartNewRound()
		{
			lock (_sessionLock)
			{
				CurrentRound++;

				_activeBombs.Clear();
				_activeExplosions.Clear();
				_activeEnemies.Clear();
				_activePowerUps.Clear();

				Map = new GameMap();

				int playerIndex = 0;
				foreach (var kvp in _players)
				{
					var (spawnX, spawnY) = Map.GetSpawnPosition(playerIndex);
					kvp.Value.Respawn(spawnX, spawnY);
					playerIndex++;
				}

				SpawnEnemies(3);

				Console.WriteLine($"[GameSession] Starting round {CurrentRound}");
			}
		}

		#endregion

		#region Helper Methods

		private string? GetPlayerIdByPlayer(IPlayer player)
		{
			foreach (var kvp in _players)
			{
				if (kvp.Value.Id == player.Id)
				{
					return kvp.Key;
				}
			}
			return null;
		}

		public Dictionary<string, int> GetRoundWins()
		{
			return new Dictionary<string, int>(_roundWins);
		}

		public GameStatistics GetGameStatistics()
		{
			return new GameStatistics
			{
				SessionId = SessionId,
				PlayerCount = _players.Count,
				CurrentRound = CurrentRound,
				ActiveBombs = _activeBombs.Count,
				ActiveExplosions = _activeExplosions.Count,
				ActiveEnemies = _activeEnemies.Count,
				ActivePowerUps = _activePowerUps.Count,
				State = State,
				Duration = StartedAt.HasValue
					? (EndedAt ?? DateTime.UtcNow) - StartedAt.Value
					: TimeSpan.Zero
			};
		}

		#endregion

		#region Cleanup

		public void Dispose()
		{
			_gameUpdateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
			_gameUpdateTimer?.Dispose();
			_gameUpdateTimer = null;

			_gameSubject.ClearObservers();
			_players.Clear();
			_activeBombs.Clear();
			_activeExplosions.Clear();
			_activeEnemies.Clear();
			_activePowerUps.Clear();

			Console.WriteLine($"[GameSession] Session {SessionId} disposed");
		}

		#endregion
	}

#region Supporting Classes

	#endregion
}